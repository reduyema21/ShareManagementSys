using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaccoShareManagementSys.Services;
using System.Security.Claims;

namespace SaccoShareManagementSys.Areas.Identity.Pages.Account
{
    public class PendingApprovalModel : PageModel
    {
        private readonly IShareholderService _shareholderService;

        public PendingApprovalModel(IShareholderService shareholderService)
        {
            _shareholderService = shareholderService;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }
            var shareholder = await _shareholderService.GetByUserIdAsync(userId);
            if (shareholder != null && shareholder.IsApproved)
            {
                // approved — go to member dashboard
                return RedirectToAction("Index", "Member");
            }
            return Page(); // still pending
        }
    }
}
