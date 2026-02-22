using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace InfosecAcademyBudgetManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Department { get; set; }
    }
}
