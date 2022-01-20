using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public RegisterModel(UserManager<User> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }
        [BindProperty]
        public RegisterViewModel RegisterViewModel { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Validating Email address (optional)

            // Create the user
            var user = new User()
            {
                Email = RegisterViewModel.Email,
                UserName = RegisterViewModel.Email,
                Department = RegisterViewModel.Department,
                Position = RegisterViewModel.Position
            };

            var result = await _userManager.CreateAsync(user, RegisterViewModel.Password);
            if (result.Succeeded)
            {
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail",
                    values: new { userId = user.Id, token = confirmationToken });

                await _emailService.SendAsync("info@tomasi-developing.ch", user.Email, "Please confirm your email",
                    $"Please click on this link to confirm your email address: {confirmationLink}");

                return Redirect("/Account/Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return Page();
            }
        }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        [Required]
        [DataType(dataType:DataType.Password)]
        public string Password { get; set; }
        [Required] 
        public string Department { get; set; }
        [Required]
        public string Position { get; set; }
    }
}

