namespace InfosecAcademyBudgetManagement.Interface
{
    public interface IEmailPasswordProtector
    {
        string Protect(string plainText);
        string? Unprotect(string cipherText);
    }
}
