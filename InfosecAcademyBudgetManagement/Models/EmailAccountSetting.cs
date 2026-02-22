using System.ComponentModel.DataAnnotations;

namespace InfosecAcademyBudgetManagement.Models
{
    public class EmailAccountSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string SenderEmail { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SenderName { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string EncryptedPassword { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
