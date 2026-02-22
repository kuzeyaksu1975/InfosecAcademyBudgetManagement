using System.ComponentModel.DataAnnotations;

namespace InfosecAcademyBudgetManagement.Models
{
    public class LoginViewModel
    {
        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni hatırla")]
        public bool RememberMe { get; set; }
    }

    public class LoginWith2faViewModel
    {
        [Required]
        [StringLength(7, MinimumLength = 6)]
        [Display(Name = "Authenticator Kodu")]
        public string TwoFactorCode { get; set; } = string.Empty;

        [Display(Name = "Bu cihazı hatırla")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class Enable2faViewModel
    {
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;
        public string QrCodeImageBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "Kod 6 veya 7 karakter olmalıdır.")]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; } = string.Empty;
    }

    public class UserProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;
        public string QrCodeImageBase64 { get; set; } = string.Empty;
    }

    public class ProfileEnable2faViewModel
    {
        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "Kod 6 veya 7 karakter olmalıdır.")]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifre alanları eşleşmiyor.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class AdminCreateUserViewModel
    {
        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ad")]
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Departman")]
        [StringLength(100, ErrorMessage = "Departman en fazla 100 karakter olabilir.")]
        public string? Department { get; set; }

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        public string Role { get; set; } = "Kullanıcı";
    }

    public class AdminEditUserViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ad")]
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Departman")]
        [StringLength(100, ErrorMessage = "Departman en fazla 100 karakter olabilir.")]
        public string? Department { get; set; }

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        public string Role { get; set; } = "Kullanıcı";

        [Display(Name = "2FA Etkin")]
        public bool TwoFactorEnabled { get; set; }
    }

    public class AdminUserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Kullanıcı";
        public bool TwoFactorEnabled { get; set; }
    }
}
