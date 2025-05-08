using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shopping_Tutorial.Repository.Validation;

namespace Shopping_Tutorial.Models;

public class ProductModel
{
    [Key]
    public int Id { get; set; }
    [Required( ErrorMessage = "Yêu cầu nhập Tên Sản phẩm")]
    public string Name { get; set; }

    public string Slug { get; set; }
    [Required( ErrorMessage = "Yêu cầu nhập Mô tả Sản phẩm")]
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }

    public int BrandId { get; set; }

    public int CategoryId { get; set; }

    public CategoryModel Category { get; set; }
    public BrandModel Brand { get; set; }
    public string Image { get; set; }

    public int Quantity { get; set; }
    public int Sold { get; set; }

    [NotMapped]
    [FileExtension]
    public IFormFile? ImageUpload { get; set; }
}
