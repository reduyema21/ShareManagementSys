using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SaccoShareManagementSys.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress]
            public string? Email { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email!);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Always redirect to confirmation page to avoid revealing info
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Build reset link
            var resetLink = Url.Page(
                "/Account/ResetPassword",        // ResetPassword page
                pageHandler: null,
                values: new { token, email = Input.Email },
                protocol: Request.Scheme);

            // Send email (replace this with your email service)
            System.Diagnostics.Debug.WriteLine($"Password reset link: {resetLink}");

            // TODO: Replace the above line with your real email sending logic

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

    }
}