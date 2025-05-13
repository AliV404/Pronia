using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilites.Enums;
using Pronia.Utilites.Extensions;
using Pronia.ViewModels;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Moderator")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<GetProductVM> productVMs = await _context.Products.Select(p => new GetProductVM
            {
                Name = p.Name,
                SKU = p.SKU,
                Id = p.Id,
                Price = p.Price,
                CategoryName = p.Category.Name,
                MainImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image
            }).ToListAsync();
            return View(productVMs);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await _context.Categories.ToListAsync()
            };

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();

            if (!ModelState.IsValid) return View(productVM);

            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);

            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category doesnt exist!");
                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateType("image/")){
                ModelState.AddModelError(nameof(CreateProductVM.MainPhoto), "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateSize(FileSize.KB, 500))
            {
                ModelState.AddModelError(nameof(CreateProductVM.MainPhoto), "File size can not be greater than 500 KB");
                return View(productVM);
            }

            bool nameResult = await _context.Products.AnyAsync(p => p.Name == productVM.Name);
            if (nameResult)
            {
                ModelState.AddModelError(nameof(CreateProductVM.Name), $"Product {productVM.Name} already exists");
                return View(productVM);
            }

            ProductImage main = new ProductImage
            {
                Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now,
            };

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price.Value,
                SKU = productVM.SKU,
                Description = productVM.Description,
                CategoryId = productVM.CategoryId.Value,
                CreatedAt = DateTime.Now,
                ProductImages = new List<ProductImage> { main }
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                SKU = product.SKU,
                Description= product.Description,
                CategoryId = product.CategoryId,
                Price = product.Price,
                PrimaryImage = product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                Categories = await _context.Categories.ToListAsync()
            };

            return View(productVM);
        }

        [HttpPost]

        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            if (!ModelState.IsValid) return View(productVM);

            if (productVM.MainPhoto != null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.MainPhoto), "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(FileSize.KB, 500))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.MainPhoto), "File size can not be greater than 500 KB");
                    return View(productVM);
                }
            }

            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);

            if (!result)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.CategoryId), "Category doesn't exist");
                return View(productVM);
            }

            bool nameResult = await _context.Products.AnyAsync(p => p.Name == productVM.Name && p.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.Name), "Product already exist");
                return View(productVM);
            }

            Product? existed = await _context.Products
                .Include(p=> p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if(productVM.MainPhoto != null)
            {
                ProductImage main = new ProductImage
                {
                    Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                    IsPrimary = true,
                    CreatedAt = DateTime.Now
                };
                ProductImage? existedMain = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == true);
                existedMain.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedMain);
                existed.ProductImages.Add(main);
            }

            existed.Name = productVM.Name;
            existed.Price = productVM.Price.Value;
            existed.Description = productVM.Description;
            existed.SKU = productVM.SKU;
            existed.CategoryId = productVM.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            foreach (ProductImage proImage in product.ProductImages)
            {
                proImage.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
