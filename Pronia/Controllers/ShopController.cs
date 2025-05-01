using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    public class ShopController : Controller
    {
        public readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product? product = _context.Products
                .Include(p => p.ProductImages.OrderByDescending(pi => pi.IsPrimary))
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);
            

            if (product is null) return NotFound();

            DetailVM detailVM = new DetailVM
            {
                Product = product,
                RelatedProducts = _context.Products.
                Take(8)
                .Where(p  => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary !=null)).ToList()
            };

            return View(detailVM);
        }
    }
}
