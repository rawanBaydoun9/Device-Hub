using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(80)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}