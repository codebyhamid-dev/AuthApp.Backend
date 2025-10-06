using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AuthApp.Backend.AuthApp.Backend.Domain
{
    public class ApplicationUser:IdentityUser
    {
        [PersonalData]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }  // ✅ Custom field
    }
}
