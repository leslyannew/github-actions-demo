using System.Security.Cryptography.X509Certificates;
using github_actions_demo.Entities;
using github_actions_demo.Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Saml2Authentication;
using X509StoreFinder;

namespace github_actions_demo.Setup;
public static class AuthenticationConfigurationExtension
{
    private static readonly string[] configureOptions = new[] { "OTS", "Louisiana", "template" };

    public static IServiceCollection AddSaml2(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Microsoft.AspNetCore.Authentication.AuthenticationBuilder authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;

            options.DefaultChallengeScheme = Saml2Defaults.AuthenticationScheme;
        })
        .AddSaml2(options =>
        {
            options.AuthenticationScheme = Saml2Defaults.AuthenticationScheme;

            options.SignInScheme = IdentityConstants.ApplicationScheme;

            options.MetadataAddress = configuration.GetValue<string>("App:Saml:MetadataAddress");
            options.ForceAuthn = configuration.GetValue<bool>("App:Saml:ForceAuthn");

            //must match with any existying SP metadata file
            options.EntityId = configuration.GetValue<string>("App:Saml:EntityId");
            options.RequireMessageSigned = false;
            options.WantAssertionsSigned = true;
            options.AuthenticationRequestSigned = true;

            //signin
            options.Saml2CookieName = configuration.GetValue<string>("App:Saml:CookieName");
            options.RequestedAuthnContext = RequestedAuthnContextTypes.ProtectedFormsAuthentication();
            options.ResponseProtocolBinding = Saml2ResponseProtocolBinding.Artifact;

            //SP metadata file generation
            options.CreateMetadataFile = configuration.GetValue<bool>("App:Saml:GenerateMetadataFile");
            options.DefaultMetadataFileName = configuration.GetValue<string>("App:Saml:MetadataFileName");

            X509Certificate2 certificate;
            string? certificateSerialNumber = configuration.GetValue<string>("App:Saml:Certificate:SerialNumber");

            certificate = !string.IsNullOrEmpty(certificateSerialNumber)
                ? X509.LocalMachine.My.FindBySerialNumber.Find(certificateSerialNumber)
                : new X509Certificate2(
                    configuration.GetValue<string>("App:Saml:Certificate:FilePath") ?? "",
                    configuration.GetValue<string>("App:Saml:Certificate:Password") ?? "",
                    X509KeyStorageFlags.Exportable);

            options.SigningCertificate = certificate;
            options.EncryptingCertificate = certificate;

            options.Metadata = new Saml2MetadataXml
            {
                ContactPersons = new ContactPerson
                {
                    Company = "OTS",
                    GivenName = "OTS",
                    Surname = "OTS",
                    EmailAddress = "opensource@la.gov",
                    ContactType = ContactType.Technical
                },
                Organization = new Organization
                {
                    OrganizationName = "Louisiana Office of Technology Services",
                    OrganizationDisplayName = "OTS",
                    OrganizationURL = new Uri("https://www.doa.la.gov/Pages/ots/index.aspx"),
                    Language = "en"
                },
                //optional
                UiInfo = new UiInfo
                {
                    Language = "en",
                    DisplayName = "OTS",
                    Description = "Louisiana Office of Technology Services",
                    InformationURL = new Uri("https://https://docs.ots.la.gov"),
                    PrivacyStatementURL = new Uri("https://docs.ots.la.gov"),
                    LogoHeight = "50",
                    LogoWidth = "100",
                    LogoUriValue = new Uri("https://s3.amazonaws.com/ots-app-img/ots2.png"),
                    KeywordValues = configureOptions,
                },
                //optional
                AttributeConsumingService = new AttributeConsumingService
                {
                    ServiceDescriptions = "Sign-in for OTS appliction templates",
                    ServiceNames = "OTS Application Template",

                    RequestedAttributes = new[]
                    {
                        new RequestedAttribute
                        {
                            Name ="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                            NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                            FriendlyName = "E-Mail Address",
                            IsRequiredField= true
                        },
                        new RequestedAttribute
                        {
                            Name ="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
                            NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                            FriendlyName = "Surname",
                            IsRequiredField= true
                        },
                        new RequestedAttribute
                        {
                            Name ="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
                            NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                            FriendlyName = "Given Name",
                            IsRequiredField= true
                        }
                    }
                }
            };

            //events
            options.Events.OnTicketReceived = context =>
            {
                return Task.CompletedTask;
            };

            options.Events.OnRemoteFailure = context =>
            {
                context.Response.Redirect("/Account/AuthenticationError");
                context.HandleResponse();
                return Task.CompletedTask;
            };
        });


        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = configuration.GetValue<string>("App:CookieName");
            options.LoginPath = "/";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AuthorizationError";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        //add session
        services.AddSession(options =>
         {
             options.IdleTimeout = TimeSpan.FromSeconds(10);
             options.Cookie.HttpOnly = true;
             options.Cookie.IsEssential = true;
         });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}

