using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models;

public class CouponModel
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public int Quantity { get; set; }
    public int Status { get; set; }
}
