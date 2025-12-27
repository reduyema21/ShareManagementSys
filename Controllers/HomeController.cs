using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaccoShareManagementSys.Services.SaccoManagement.Services;
using SaccoShareManagementSys.ViewModels;

[Authorize] // ensures only authenticated users can access
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(
        IDashboardService dashboardService,
        ILogger<HomeController> logger,
        UserManager<IdentityUser> userManager)
    {
        _dashboardService = dashboardService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Redirect("/Identity/Account/Login");

        // ROLE REDIRECTS
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            // Admin sees the main dashboard
            try
            {
                var viewModel = await _dashboardService.GetDashboardDataAsync();
                return View(viewModel); // main dashboard view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard.";
                return View(new DashboardViewModel());
            }
        }

        if (await _userManager.IsInRoleAsync(user, "Member"))
        {
            // Members redirected to member dashboard
            return RedirectToAction("Index", "Member");
        }

        // Unknown role → access denied
        return Redirect("/Identity/Account/AccessDenied");
    }
}
