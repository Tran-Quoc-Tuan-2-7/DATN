using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
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
            orderItem.UserName = userEmail;
            orderItem.Status = 1;
            orderItem.CreatedDate = DateTime.Now;

            _dataContext.Add(orderItem);
            _dataContext.SaveChanges();
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            foreach (var cart in cartItems)
            {
                var orderdetails = new OrderDetails
                {
                    UserName = userEmail,
                    OrderCode = ordercode,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };

                _dataContext.Add(orderdetails);
            }

            await _dataContext.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Đã tạo đơn hàng thành công";
            return RedirectToAction("Index", "Cart");

        }
        return View();
    }
}

