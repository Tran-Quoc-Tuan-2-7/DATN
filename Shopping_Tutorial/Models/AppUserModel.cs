using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Shopping_Tutorial.Models
{
    public class AppUserModel : IdentityUser
    {
        public string? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public IdentityRole? Role { get; set; }
    }
}
