using Microsoft.AspNetCore.Mvc;
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
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
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

            TempData["success"] = "Add Item to cart Successfully";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        //Ham giam so luong
        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity > 1)
            {
                --cartItem.Quantity;
            }
            else
            {
                cart.RemoveAll(p =>  p.ProductId == Id);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }

            else
            {
                HttpContext.Session.SetJson("Cart", cart);

            }

            TempData["success"] = "Decrease Item quantity to cart Successfully";

            return RedirectToAction("Index");
        }

        //Ham tang so luong
        public async Task<IActionResult> Increase(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItem.Quantity > 0)
            {
                ++cartItem.Quantity;
            }
            else
            {
                cart.RemoveAll(p => p.ProductId == Id);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }

            else
            {
                HttpContext.Session.SetJson("Cart", cart);

            }

            TempData["success"] = "Increase Item quantity to cart Successfully";

            return RedirectToAction("Index");
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

            TempData["success"] = "Remove Item to cart Successfully";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear(int Id)
        {
            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Clear all Item to cart Successfully";

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

    }


}
