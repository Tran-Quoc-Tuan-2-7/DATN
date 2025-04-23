using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult>Details(int Id)
        {
            if (Id == null) return RedirectToAction("/Index");

            var productsById = _dataContext.Products.Where(p => p.Id == Id).FirstOrDefault();

            return View(productsById);
        }

    }
}
