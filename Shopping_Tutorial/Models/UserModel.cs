using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required]        
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password), Required]
        public string Password { get; set; }
    }
}
