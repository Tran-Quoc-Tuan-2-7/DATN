using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models;

public class OrderModel
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public decimal ShippingCost { get; set; }
    public string CouponCode { get; set; }

    public string UserName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int Status { get; set; }
    public int StoreId { get; set; }
    [ForeignKey("StoreId")]
    public StoreModel Store { get; set; }
}
