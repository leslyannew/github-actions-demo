using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace github_actions_demo.Features.Home;
public class HomeController(ILogger<HomeController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {

        if (User?.Identity == null || !User.Identity.IsAuthenticated)
        {
            logger.HomePageRedirected();
            return RedirectToAction("Index", "Account");
        }
        logger.HomePageVisited(User.Identity?.Name ?? "anonymous user");

        return View();
    }

    [HttpGet]
    [FeatureGate(FeatureFlags.PrivacyPage)]
    public IActionResult Privacy()
    {
        logger.PrivacyPageVisited(User?.Identity?.Name ?? "anonymous user");

        return View();
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
