using Microsoft.AspNetCore.DataProtection;
using InfosecAcademyBudgetManagement.Interface;

namespace InfosecAcademyBudgetManagement.Helpers
{
    public class EmailPasswordProtector : IEmailPasswordProtector
    {
        private readonly IDataProtector _protector;

        public EmailPasswordProtector(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("InfosecAcademyBudgetManagement.EmailPassword.v1");
        }

        public string Protect(string plainText)
        {
            return _protector.Protect(plainText);
        }

        public string? Unprotect(string cipherText)
        {
            try
            {
                return _protector.Unprotect(cipherText);
            }
            catch
            {
                return null;
            }
        }
    }
}
