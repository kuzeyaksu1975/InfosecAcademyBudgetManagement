namespace InfosecAcademyBudgetManagement.Interface
{
    public interface IEmailServiceHelper
    {
        Task SendAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
        Task SendWithSettingsAsync(Models.EmailAccountSetting settings, string plainPassword, string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    }
}
