using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;

        public SizeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Size> sizes = await _context.Sizes.ToListAsync();
            return View(sizes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Size size)
        {
            if (!ModelState.IsValid) return View();

            bool result = await _context.Sizes.AnyAsync(s => s.Name == size.Name && s.Id != size.Id);
            if (result)
            {
                ModelState.AddModelError(nameof(Size.Name), $"{size.Name} already exists!");
                return View();
            }

            size.CreatedAt = DateTime.Now;

            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null && id < 1) return BadRequest();

            Size? size = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);
            if (size is null) return NotFound();

            return View(size);

        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, Size size)
        {
            if (!ModelState.IsValid) return View();

            bool result =  _context.Sizes.Any(s => s.Name == size.Name && s.Id != id);

            if (result)
            {
                ModelState.AddModelError(nameof(Size.Name), $"{size.Name} already exists");
                return View();
            }

            Size? existed = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);

            if (existed.Name == size.Name) return RedirectToAction(nameof(Index));

            existed.Name = size.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }
    }
}
