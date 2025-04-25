using Microsoft.AspNetCore.Identity;

namespace Shopping_Tutorial.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occupation {  get; set; }
    }
}
