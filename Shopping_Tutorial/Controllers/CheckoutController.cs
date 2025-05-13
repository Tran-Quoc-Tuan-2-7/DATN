using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            orderItem.ShippingCost = shippingPrice;
            orderItem.UserName = userEmail;
            orderItem.Status = 1;
            orderItem.CreatedDate = DateTime.Now;

            _dataContext.Add(orderItem);
            _dataContext.SaveChanges();
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            foreach (var cart in cartItems)
            {
                var orderdetail = new OrderDetails();
                orderdetail.UserName = userEmail;
                orderdetail.OrderCode = ordercode;
                orderdetail.ProductId = cart.ProductId;
                orderdetail.Price = cart.Price;
                orderdetail.Quantity = cart.Quantity;

                //update product quantity
                var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                product.Quantity -= cart.Quantity;
                product.Sold += cart.Quantity;

                _dataContext.Update(product);
                _dataContext.Add(orderdetail);
                _dataContext.SaveChanges();
            }

            await _dataContext.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Đã tạo đơn hàng thành công";
            return RedirectToAction("Index", "Cart");

        }
    }
}

