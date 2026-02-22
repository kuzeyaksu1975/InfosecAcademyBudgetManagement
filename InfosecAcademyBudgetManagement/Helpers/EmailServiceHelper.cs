using InfosecAcademyBudgetManagement.Data;
using InfosecAcademyBudgetManagement.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace InfosecAcademyBudgetManagement.Helpers
{
    public class EmailServiceHelper : IEmailServiceHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailPasswordProtector _passwordProtector;

        public EmailServiceHelper(ApplicationDbContext context, IEmailPasswordProtector passwordProtector)
        {
            _context = context;
            _passwordProtector = passwordProtector;
        }

        public async Task SendAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var settings = await _context.Set<Models.EmailAccountSetting>().AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            if (settings is null)
            {
                throw new InvalidOperationException("E-posta ayarları bulunamadı.");
            }

            var password = _passwordProtector.Unprotect(settings.EncryptedPassword);
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("E-posta şifresi çözümlenemedi.");
            }

            await SendWithSettingsAsync(settings, password, toEmail, subject, body, isHtml, cancellationToken);
        }

        public async Task SendWithSettingsAsync(Models.EmailAccountSetting settings, string plainPassword, string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName ?? settings.SenderEmail, settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = isHtml
                ? new TextPart("html") { Text = body }
                : new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            var secureSocketOption = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, secureSocketOption, cancellationToken);
            await client.AuthenticateAsync(settings.Username, plainPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
