using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public IActionResult Index()
        {
            return View();
        }

        public ProductController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(products);
        }

        public async Task<IActionResult>Details(int Id)
        {
            if (Id == null) return RedirectToAction("/Index");

            var productsById = _dataContext.Products.Where(p => p.Id == Id).FirstOrDefault();

            //san pham lien quan
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4).
                ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(productsById);
        }

    }
}
