using InfosecAcademyBudgetManagement.Data;
using InfosecAcademyBudgetManagement.Helpers;
using InfosecAcademyBudgetManagement.Interface;
using InfosecAcademyBudgetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfosecAcademyBudgetManagement.Controllers
{
    [Authorize(Roles = "Yönetici")]
    public class EmailSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailPasswordProtector _passwordProtector;
        private readonly IEmailServiceHelper _emailServiceHelper;

        public EmailSettingsController(ApplicationDbContext context, IEmailPasswordProtector passwordProtector, IEmailServiceHelper emailServiceHelper)
        {
            _context = context;
            _passwordProtector = passwordProtector;
            _emailServiceHelper = emailServiceHelper;
        }

        public async Task<IActionResult> Index()
        {
            var entity = await _context.EmailAccountSettings.AsNoTracking().FirstOrDefaultAsync();
            if (entity is null)
            {
                return View(new EmailSettingsViewModel());
            }

            var model = new EmailSettingsViewModel
            {
                SmtpHost = entity.SmtpHost,
                SmtpPort = entity.SmtpPort,
                EnableSsl = entity.EnableSsl,
                SenderEmail = entity.SenderEmail,
                SenderName = entity.SenderName,
                Username = entity.Username,
                HasPassword = !string.IsNullOrWhiteSpace(entity.EncryptedPassword)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmailSettingsViewModel model)
        {
            var existing = await _context.EmailAccountSettings.FirstOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                model.HasPassword = existing is not null && !string.IsNullOrWhiteSpace(existing.EncryptedPassword);
                return View(model);
            }

            if (existing is null)
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(nameof(model.Password), "Yeni kayıt için şifre zorunludur.");
                    model.HasPassword = false;
                    return View(model);
                }

                existing = new EmailAccountSetting
                {
                    CreatedAt = DateTime.UtcNow
                };
                _context.EmailAccountSettings.Add(existing);
            }

            existing.SmtpHost = model.SmtpHost;
            existing.SmtpPort = model.SmtpPort;
            existing.EnableSsl = model.EnableSsl;
            existing.SenderEmail = model.SenderEmail;
            existing.SenderName = model.SenderName;
            existing.Username = model.Username;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                existing.EncryptedPassword = _passwordProtector.Protect(model.Password);
            }
            else if (string.IsNullOrWhiteSpace(existing.EncryptedPassword))
            {
                ModelState.AddModelError(nameof(model.Password), "Şifre zorunludur.");
                model.HasPassword = false;
                return View(model);
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "E-posta ayarları kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestEmail(EmailSettingsViewModel model)
        {
            var existing = await _context.EmailAccountSettings.AsNoTracking().FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(model.TestRecipientEmail))
            {
                ModelState.AddModelError(nameof(model.TestRecipientEmail), "Test alıcı e-posta zorunludur.");
            }

            if (!ModelState.IsValid)
            {
                model.HasPassword = existing is not null && !string.IsNullOrWhiteSpace(existing.EncryptedPassword);
                return View(nameof(Index), model);
            }

            var plainPassword = model.Password;
            if (string.IsNullOrWhiteSpace(plainPassword) && existing is not null && !string.IsNullOrWhiteSpace(existing.EncryptedPassword))
            {
                plainPassword = _passwordProtector.Unprotect(existing.EncryptedPassword);
            }

            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                ModelState.AddModelError(nameof(model.Password), "Test gönderimi için şifre zorunludur.");
                model.HasPassword = false;
                return View(nameof(Index), model);
            }

            var tempSettings = new EmailAccountSetting
            {
                SmtpHost = model.SmtpHost,
                SmtpPort = model.SmtpPort,
                EnableSsl = model.EnableSsl,
                SenderEmail = model.SenderEmail,
                SenderName = model.SenderName,
                Username = model.Username,
                EncryptedPassword = string.Empty
            };

            try
            {
                var subject = "InfosecAcademy - Test E-postası";
                var body = "<p>Bu bir test e-postasıdır. SMTP ayarlarınız başarıyla çalışıyor.</p>";
                await _emailServiceHelper.SendWithSettingsAsync(tempSettings, plainPassword, model.TestRecipientEmail!, subject, body, isHtml: true);
                TempData["SuccessMessage"] = "Test e-postası başarıyla gönderildi.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Test e-postası gönderilemedi: {ex.Message}");
            }

            model.Password = string.Empty;
            model.HasPassword = !string.IsNullOrWhiteSpace(plainPassword);
            return View(nameof(Index), model);
        }
    }
}
