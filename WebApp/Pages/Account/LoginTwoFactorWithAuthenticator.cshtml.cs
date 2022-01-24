using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class LoginTwoFactorWithAuthenticatorModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        [BindProperty]
        public AuthenticatorMFA AuthenticatorMfa { get; set; }

        public LoginTwoFactorWithAuthenticatorModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            this.AuthenticatorMfa = new AuthenticatorMFA();
        }

        public void OnGet(bool rememberMe)
        {
            this.AuthenticatorMfa.SecurityCode = string.Empty;
            this.AuthenticatorMfa.RememberMe = rememberMe;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(this.AuthenticatorMfa.SecurityCode,
                this.AuthenticatorMfa.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Authenticator2FA", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Authenticator2FA", "Failed to login");
                }

                return Page();
            }

        }
    }

    public class AuthenticatorMFA
    {
        [Required]
        [Display(Name = "Code")]
        public string SecurityCode { get; set; }

        public bool RememberMe { get; set; }
    }
}
