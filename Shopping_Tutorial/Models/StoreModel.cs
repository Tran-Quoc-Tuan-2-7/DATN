using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models;

public class StoreModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Address { get; set; }
}
