using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LoginModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty] 
        public CredentialViewModel Credential { get; set; }

        [BindProperty] 
        public IEnumerable<AuthenticationScheme> ExternalLoginProviders { get; set; }


        public async Task OnGetAsync()
        {
            this.ExternalLoginProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(this.Credential.Email, this.Credential.Password,
                this.Credential.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginTwoFactorWithAuthenticator", new
                    {
                        RememberMe = this.Credential.RememberMe
                    });
                }
                if (result.IsLockedOut)
                {

                    ModelState.AddModelError("Login", "You are locked out");
                }
                else
                {
                    ModelState.AddModelError("Login", "Failed to login");
                }

                return Page();
            }
        }

        public IActionResult OnPostLoginExternally(string provider)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, null);
            properties.RedirectUri = Url.Action("ExternalLoginCallback", "Account");

            return Challenge(properties, provider);
        }
    }

    public class CredentialViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
