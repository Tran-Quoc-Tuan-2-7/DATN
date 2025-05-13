using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        public CartController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public IActionResult Index()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            //nhan shipping gia tu cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingCost = shippingPrice,
            };
            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        public async Task<IActionResult> Add(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems == null)
            {
                cart.Add(new CartItemModel(product));
            }

            else
            {
                cartItems.Quantity += 1;
            }

            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Thêm sản phẩm thành công";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        //Ham giam so luong
        [HttpPost]
        public async Task<IActionResult> Increase(int id)
        {
            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

            if (cartItem.Quantity > 0 && product.Quantity > cartItem.Quantity)
            {
                cartItem.Quantity++;
            }

            HttpContext.Session.SetJson("Cart", cart);

            return Json(new
            {
                success = true,
                quantity = cartItem.Quantity,
                total = cartItem.Quantity * cartItem.Price,
                grandTotal = cart.Sum(x => x.Price * x.Quantity)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Decrease(int id)
        {
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }

            HttpContext.Session.SetJson("Cart", cart);

            return Json(new
            {
                success = true,
                quantity = cartItem.Quantity,
                total = cartItem.Quantity * cartItem.Price,
                grandTotal = cart.Sum(x => x.Price * x.Quantity)
            });
        }


        //Ham Remove
        public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            cart.RemoveAll(p => p.ProductId == Id);

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }

            else
            {
                HttpContext.Session.SetJson("Cart", cart);

            }

            TempData["success"] = "Xóa sản phẩm thành công";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear(int Id)
        {
            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Xóa tất cả sản phẩm thành công";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddAjax(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem == null)
                cart.Add(new CartItemModel(product));
            else
                cartItem.Quantity += 1;

            HttpContext.Session.SetJson("Cart", cart);

            return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });
        }

        //tinh phi ship
        [HttpPost]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {
            var existingShipping = await _dataContext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
            decimal shippingPrice = 0;
            if (existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                shippingPrice = 50000;
            }
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddMinutes(30),                    
                    Secure = Request.IsHttps
                };
                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }

            return Json(new { shippingPrice });
        }

        [HttpPost]
        public IActionResult RemoveShippingCookie()
        {
            Response.Cookies.Delete("ShippingPrice");
            return Json(new { success = true });
        }
    }
}