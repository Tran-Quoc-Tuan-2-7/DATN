using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    return new ValidationResult("Trường này chỉ chấp nhận các file .jpg, .jpeg, .png, .gif");
                }
            }

            return ValidationResult.Success;
        }
    }
}
