using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers;

public class CheckoutController : Controller
{
    private readonly DataContext _dataContext;

    public CheckoutController(DataContext context)
    {
        _dataContext = context;
    }

    public async Task<IActionResult> Checkout()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == null)
        {
            return RedirectToAction("Login", "Account");
        }
        else
        {
            var ordercode = Guid.NewGuid().ToString();
            var orderItem = new OrderModel();
            orderItem.OrderCode = ordercode;

            // Nhan shipping price tu cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }
            orderItem.ShippingCost = shippingPrice;

            // Nhan coupon code tu cookie
            var coupon_code = Request.Cookies["CouponTitle"];
            orderItem.CouponCode = coupon_code;

            orderItem.UserName = userEmail;
            orderItem.Status = 1;
            orderItem.CreatedDate = DateTime.Now;

            // Lay StoreId tu cookie (hoac ban thay bang cach lay khac)
            var storeIdCookie = Request.Cookies["SelectedStoreId"];
            int storeId = 1; // mac đinh neu khong co cookie
            if (storeIdCookie != null && int.TryParse(storeIdCookie, out int parsedStoreId))
            {
                storeId = parsedStoreId;
            }
            orderItem.StoreId = storeId; // Gan storeId cho don hang

            // Them don hang
            _dataContext.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            // Lay danh sach gio hang tu session
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            foreach (var cart in cartItems)
            {
                var orderdetail = new OrderDetails();
                orderdetail.UserName = userEmail;
                orderdetail.OrderCode = ordercode;
                orderdetail.ProductId = cart.ProductId;
                orderdetail.Price = cart.Price;
                orderdetail.Quantity = cart.Quantity;

                _dataContext.Add(orderdetail);
            }

            await _dataContext.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Đã tạo đơn hàng thành công";
            return RedirectToAction("History", "Account");
        }
    }
}
