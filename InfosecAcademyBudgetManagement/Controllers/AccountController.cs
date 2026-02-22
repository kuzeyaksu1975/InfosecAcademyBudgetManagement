using System.Text;
using InfosecAcademyBudgetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace InfosecAcademyBudgetManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel
            {
                Email = "admin@local.test",
                Password = "Admin123!",
                RememberMe = true
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, rememberMe = model.RememberMe });
            }

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesap geçici olarak kilitlendi.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new LoginWith2faViewModel
            {
                RememberMe = rememberMe,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Doğrulama kodu geçersiz.");
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(await BuildProfileViewModelAsync(user));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                var profileModel = await BuildProfileViewModelAsync(user);
                return View(nameof(Profile), profileModel);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                var profileModel = await BuildProfileViewModelAsync(user);
                return View(nameof(Profile), profileModel);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable2faFromProfile(ProfileEnable2faViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                var profileModel = await BuildProfileViewModelAsync(user);
                return View(nameof(Profile), profileModel);
            }

            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                verificationCode);

            if (!isTokenValid)
            {
                ModelState.AddModelError(string.Empty, "Doğrulama kodu geçersiz.");
                var profileModel = await BuildProfileViewModelAsync(user);
                return View(nameof(Profile), profileModel);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "2FA başarıyla etkinleştirildi.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2faFromProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "2FA devre dışı bırakıldı.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult Enable2fa()
        {
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable2fa(Enable2faViewModel model)
        {
            var forward = new ProfileEnable2faViewModel
            {
                Code = model.Code
            };
            return await Enable2faFromProfile(forward);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            var currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private static string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string issuer = "InfosecAcademyBudgetManagement";
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={unformattedKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6";
        }

        private static string GenerateQrCodeBase64(string payload)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrCodeData);
            var pngBytes = pngQrCode.GetGraphic(12);
            return Convert.ToBase64String(pngBytes);
        }

        private async Task<UserProfileViewModel> BuildProfileViewModelAsync(ApplicationUser user)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var authUri = GenerateQrCodeUri(user.Email ?? user.UserName ?? "user", key!);
            return new UserProfileViewModel
            {
                Email = user.Email ?? "-",
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Department = user.Department,
                TwoFactorEnabled = user.TwoFactorEnabled,
                SharedKey = FormatKey(key!),
                AuthenticatorUri = authUri,
                QrCodeImageBase64 = GenerateQrCodeBase64(authUri)
            };
        }
    }
}
