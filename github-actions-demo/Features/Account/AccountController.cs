using System.Security.Claims;
using System.Text.RegularExpressions;
using github_actions_demo.Entities;
using github_actions_demo.Features.Home;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Saml2Authentication;

namespace github_actions_demo.Features.Account;
public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IHostEnvironment environment,
    ILogger<AccountController> logger) : Controller
{
    private readonly string TrueIdClaim = "http://la.gov/LA-True-ID";
    private readonly string SamlSidClaim = "http://saml2/sid";

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User?.Identity is not null && User.Identity.IsAuthenticated)
        {
            logger.AccountPageRedirect(User.Identity.Name ?? "user");
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLogin(string provider, Uri? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            logger.ExternalLoginVisited(provider.Replace(Environment.NewLine, ""),
            returnUrl?.AbsoluteUri.Replace(Environment.NewLine, ""));

            // If return URL comes from external source, throw exception as possible attack
            if (returnUrl != null && (!Url.IsLocalUrl(returnUrl.AbsoluteUri) || !IsValidReturnUrl(returnUrl.AbsoluteUri)))
            {
                throw new Exception("Invalid return URL");
            }

            // Request a redirect to the external login provider.
            string redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        return RedirectToAction("Index", "Home");
    }

    private bool IsValidReturnUrl(string returnUrl)
    {
        string pattern = @"^\/[a-zA-Z0-9\/\-_]*(\?[a-zA-Z0-9=&]*)?$";
        return Regex.IsMatch(returnUrl, pattern, RegexOptions.IgnoreCase);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(Uri? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            AuthenticateResult? result = await HttpContext.AuthenticateAsync();
            string? provider = result?.Properties?.Items["LoginProvider"];

            if (result?.Succeeded != true)
            {
                logger.ExternalLoginCallbackFailure(
                    User?.Identity?.Name ?? "unauthenticated user",
                    provider?.Replace(Environment.NewLine, "") ?? "unknown schema",
                    returnUrl?.AbsoluteUri.Replace(Environment.NewLine, ""));

                return RedirectToAction("AuthenticationError", "Account");
            }

            if (result.Principal?.Claims == null)
            {
                logger.ExternalLoginCallbackNoClaimsFound();
                return RedirectToAction("AuthenticationError", "Account");
            }

            logger.ExternalLoginCallbackSuccess(
                    User?.Identity?.Name ?? "unauthenticated user",
                    provider?.Replace(Environment.NewLine, "") ?? "unknown schema",
                    returnUrl?.AbsoluteUri.Replace(Environment.NewLine, ""));


            bool identityResult = await AutoProvisionUserAsync(result.Principal, result.Properties!);

            if (!identityResult)
            {
                return RedirectToAction("AuthorizationError", "Account");
            }
            if (Url.IsLocalUrl(returnUrl?.AbsoluteUri))
            {
                return LocalRedirect(returnUrl.AbsoluteUri);
            }
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> UserClaims()
    {
        AuthenticateResult result = await HttpContext.AuthenticateAsync();
        IEnumerable<Claim>? claims = result.Principal?.Claims;

        return View(claims);

    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task Logout()
    {
        AuthenticateResult result = await HttpContext.AuthenticateAsync();
        string? scheme = result.Ticket?.AuthenticationScheme;
        AuthenticationProperties? properties = result.Properties;
        string? provider = properties?.Items["LoginProvider"];
        await HttpContext.SignOutAsync(scheme);
        await HttpContext.SignOutAsync(provider, properties);
    }

    //user is authenticated but not authorized
    [HttpGet]
    public async Task<IActionResult> AuthorizationError()
    {
        if (User?.Identity is not null && !User.Identity.IsAuthenticated && HttpContext is not null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
        return View();
    }

    //user is not authenticated
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AuthenticationError()
    {
        return View();
    }

    private async Task<bool> AutoProvisionUserAsync(ClaimsPrincipal claims,
        AuthenticationProperties properties
        )
    {
        AuthenticateResult result = await HttpContext.AuthenticateAsync();
        string nameId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        logger.StartUserLookup(nameId.Replace(Environment.NewLine, ""));

        // lookup our user and external provider info
        ApplicationUser? user = await userManager.FindByLoginAsync(Saml2Defaults.AuthenticationScheme, nameId);

        //user already exists in backing store
        if (user != null)
        {
            user.LastLoginTime = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);

            logger.UserLookupResultFound(
                nameId.Replace(Environment.NewLine, ""),
                user.LastLoginTime ?? DateTimeOffset.UtcNow);
        }
        //new user that needs to be added to backing store
        else
        {
            logger.UserLookupResultNotFound(
                nameId.Replace(Environment.NewLine, ""));

            string firstName = claims?.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            string lastName = claims?.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
            string email = claims?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            string trueId = claims?.FindFirst(TrueIdClaim)?.Value ?? string.Empty;

            user = new ApplicationUser
            {
                UserName = nameId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                LastLoginTime = DateTimeOffset.UtcNow
            };

            //enable user automatically if in local development
            //DO NOT DO THIS IN PRODUCTION or any other environment
            //this is only for local development
            if (environment.IsDevelopment())
            {
                user.IsEnabled = true;
            }

            IdentityResult response = await userManager.CreateAsync(user);

            if (!response.Succeeded)
            {
                logger.UserCreationError(
                    user.UserName.Replace(Environment.NewLine, ""),
                    response.Errors.First().Description);

                return false;
            }

            logger.UserCreated(user.UserName.Replace(Environment.NewLine, ""));
            logger.StartUserAddClaim(user.UserName.Replace(Environment.NewLine, ""));

            response = await userManager.AddClaimsAsync(user, new List<Claim>
                {
                    new(ClaimTypes.GivenName, firstName),
                    new(ClaimTypes.Surname, lastName),
                    new(ClaimTypes.Email, email)
                });

            if (!response.Succeeded)
            {
                logger.UserAddClaimsError(
                    user.UserName.Replace(Environment.NewLine, ""),
                    response.Errors.First().Description);

                return false;
            }

            logger.UserAddClaimSuccessful(user.UserName.Replace(Environment.NewLine, ""));
            logger.StartUserAddLogin(user.UserName.Replace(Environment.NewLine, ""));

            response = await userManager.AddLoginAsync(user, new UserLoginInfo(
                Saml2Defaults.AuthenticationScheme, nameId, Saml2Defaults.AuthenticationScheme));

            if (!response.Succeeded)
            {
                logger.UserAddLoginError(
                    user.UserName.Replace(Environment.NewLine, ""),
                    response.Errors.First().Description);

                return false;
            }

            if (!user.IsEnabled)
            {
                logger.UserIsNotEnabled(
                    user.UserName.Replace(Environment.NewLine, ""));

                return false;
            }
        }

        // store saml sid
        //this is needed along with nameid
        //for user logout from saml2
        string sid = claims?.FindFirst(SamlSidClaim)?.Value ?? string.Empty;

        var localClaims = new List<Claim>
            {
                new(SamlSidClaim, sid)
            };

        logger.UserAddSidForLogout(user.UserName!.Replace(Environment.NewLine, ""));
        await signInManager.SignInWithClaimsAsync(user, properties, localClaims);

        //enable user automatically if in local development
        //DO NOT DO THIS IN PRODUCTION or any other environment
        //this is only for local development
        if (environment.IsDevelopment())
        {
            user.IsEnabled = true;
        }

        return true;
    }
}
