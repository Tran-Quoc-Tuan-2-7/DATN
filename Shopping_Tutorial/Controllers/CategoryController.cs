using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers;

public class CategoryController : Controller
{
    private readonly DataContext _dataContext;
    public CategoryController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string endprice = "", string startprice = "")
    {
        CategoryModel category = _dataContext.Categories.Where(c =>c.Slug == Slug).FirstOrDefault();

        if (category == null) return RedirectToAction("Index");

        //lay tat ca san pham
        IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryId == category.Id);

        var count = await productsByCategory.CountAsync();
        if (count > 0) 
        {
            if(sort_by == "price_increase")
            {
                productsByCategory = productsByCategory.OrderBy(p => p.Price);
            }

            else if (sort_by == "price_decrease")
            {
                productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
            }

            else if (sort_by == "price_newest")
            {
                productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
            }

            else if (sort_by == "price_oldest")
            {
                productsByCategory = productsByCategory.OrderBy(p => p.Id);
            }
            //loc gia
            else if(startprice != "" && endprice != "")
            {
                decimal startPriceValue;
                decimal endPriceValue;

                if(decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                {
                    productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
                else
                {
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                }
            }

            else
            {
                productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
            }
        }

        return View(await productsByCategory.ToListAsync());
    }
}
