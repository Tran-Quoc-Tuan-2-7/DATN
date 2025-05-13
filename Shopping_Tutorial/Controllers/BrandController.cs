using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string endprice = "", string startprice = "")
        {
            BrandModel brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();

            if (brand == null) return RedirectToAction("Index");

            IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);

            var count = await productsByBrand.CountAsync();
            if (count > 0)
            {
                if (sort_by == "price_increase")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Price);
                }

                else if (sort_by == "price_decrease")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
                }

                else if (sort_by == "price_newest")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }

                else if (sort_by == "price_oldest")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Id);
                }

                else if (startprice != "" && endprice != "")
                {
                    decimal startPriceValue;
                    decimal endPriceValue;

                    if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                    {
                        productsByBrand = productsByBrand.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                    }
                }

                else
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }
            }

            return View(await productsByBrand.ToListAsync());
        }
    }
}
