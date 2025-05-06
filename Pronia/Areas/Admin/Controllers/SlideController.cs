using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller

    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.Where(s => s.IsDeleted == false).ToListAsync();
            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            if (!slide.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError(nameof(Slide.Photo), "File type is incorrect");
                return View();
            }
            if (slide.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(Slide.Photo), "File size should be less than 2MB");
                return View();
            }
            bool result = await _context.Slides.AnyAsync(s => s.Order == slide.Order);
            if (result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slide.Order} already exists");
                return View();
            }
            string fileName = string.Concat(Guid.NewGuid().ToString(), slide.Photo.FileName.Substring(slide.Photo.FileName.LastIndexOf('.')));
            string path = Path.Combine(_env.WebRootPath, "assets", "images", "website-images", fileName);

            FileStream fl = new FileStream(path, FileMode.Create);
            await slide.Photo.CopyToAsync(fl);

            slide.Image = fileName;
            slide.CreatedAt = DateTime.Now;

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Slide? slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (slide is null) return NotFound();

            if (slide.IsDeleted)
            {
                slide.IsDeleted = false;
            }
            else
            {
                slide.IsDeleted = true;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    
      

       
    }
}