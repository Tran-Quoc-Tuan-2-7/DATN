using System.ComponentModel.DataAnnotations;
using Shopping_Tutorial.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models;

public class ContactModel
{
    [Key]
    [Required]
    public string Name { get; set; }

    [Required]
    public string Map { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string Email { get; set; }

    public string Description { get; set; }

    public string LogoImg { get; set; }

    [NotMapped]
    [FileExtension]
    public IFormFile? ImageUpload { get; set; }
}
