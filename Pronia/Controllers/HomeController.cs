using Microsoft.AspNetCore.Mvc;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        public readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            List<Slide> slides = new List<Slide>
            {
                new Slide
                {
                    Title = "New Plant (1)",
                    SubTitle = "45% OFF",
                    Description = "Pronia, With 100% Natural, Organic & Plant Shop.",
                    Order = 3,
                    Image = "plant1.jpg"
                },
                new Slide
                {
                    Title = "New Plant (2)",
                    SubTitle = "55% OFF",
                    Description = "Pronia, With 100% Natural, Organic & Plant Shop.",
                    Order = 1,
                    Image = "plant2.jpg"
                },
                new Slide
                {
                    Title = "New Plant (3)",
                    SubTitle = "65% OFF",
                    Description = "Pronia, With 100% Natural, Organic & Plant Shop.",
                    Order = 2,
                    Image = "plant3.jpg"
                }
            };
            _context.Slides.AddRange(slides);
            _context.SaveChanges();

            HomeVM homeVM = new HomeVM
            {
                Slides = slides.OrderBy(s => s.Order).Take(2).ToList(),
            };

            return View(homeVM);
        }
    }
}
