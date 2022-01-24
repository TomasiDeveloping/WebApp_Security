using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    [Authorize]
    public class AuthenticatorWithMFASetupModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        [BindProperty]
        public SetupMFAViewModel ViewModel { get; set; }

        [BindProperty]
        public bool Succeeded { get; set; }

        public AuthenticatorWithMFASetupModel(UserManager<User> userManager)
        {
            _userManager = userManager;
            this.ViewModel = new SetupMFAViewModel();
            this.Succeeded = false;
        }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(base.User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            this.ViewModel.Key = key;
            this.ViewModel.QRCodeBytes = GenerateQRCodeBytes("my web app", key, user.Email);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(base.User);
            if (await _userManager.VerifyTwoFactorTokenAsync(user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider, this.ViewModel.SecurityCode))
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                this.Succeeded = true;
            }
            else
            {
                ModelState.AddModelError("AuthenticatorSetup", "Some went wrong with the authenticator setup");
            }

            return Page();
        }

        private Byte[] GenerateQRCodeBytes(string provider, string key, string userEmail)
        {
            var qrCodeGenerater = new QRCodeGenerator();
            var qrCodeData = qrCodeGenerater.CreateQrCode(
                $"otpauth://totp/{provider}:{userEmail}?secret={key}&issuer={provider}",
                QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            return BitmapToByteArray(qrCodeImage);
        }

        private Byte[] BitmapToByteArray(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }

    public class SetupMFAViewModel
    {
        public string Key { get; set; }
        [Required]
        [Display(Name = "Code")]
        public string SecurityCode { get; set; }

        public Byte[] QRCodeBytes { get; set; }

    }
}
