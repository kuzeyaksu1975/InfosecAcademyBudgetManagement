using System.ComponentModel.DataAnnotations;

namespace InfosecAcademyBudgetManagement.Models
{
    public class EmailSettingsViewModel
    {
        [Display(Name = "SMTP Sunucu")]
        [Required(ErrorMessage = "SMTP sunucu zorunludur.")]
        [StringLength(200)]
        public string SmtpHost { get; set; } = string.Empty;

        [Display(Name = "SMTP Port")]
        [Required(ErrorMessage = "SMTP port zorunludur.")]
        [Range(1, 65535, ErrorMessage = "Port aralığı 1-65535 olmalıdır.")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "SSL/TLS Aktif")]
        public bool EnableSsl { get; set; } = true;

        [Display(Name = "Gönderen E-posta")]
        [Required(ErrorMessage = "Gönderen e-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        [StringLength(256)]
        public string SenderEmail { get; set; } = string.Empty;

        [Display(Name = "Gönderen Adı")]
        [StringLength(100)]
        public string? SenderName { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Test Alıcı E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir test e-posta adresi giriniz.")]
        public string? TestRecipientEmail { get; set; }

        public bool HasPassword { get; set; }
    }
}
