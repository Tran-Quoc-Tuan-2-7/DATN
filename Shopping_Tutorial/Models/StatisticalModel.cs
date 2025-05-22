using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models;

public class StatisticalModel
{
    public int Id { get; set; }
    public int Quantity { get; set; } // so luong ban
    public int Sold { get; set; } // so luong don
    public decimal Revenue { get; set; } // doanh thu
    public decimal Profit { get; set; } // loi nhuan

    public int StoreId { get; set; }
    [ForeignKey("StoreId")]
    public StoreModel Store { get; set; }
    public DateTime DateCreated { get; set; }
}
