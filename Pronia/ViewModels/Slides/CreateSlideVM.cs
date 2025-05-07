using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModels
{
    public class CreateSlideVM
    {
        [MaxLength(100, ErrorMessage = "slide Title must be <= 100 characters")]
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Order num can not be less than 1")]
        public int Order { get; set; }
        public IFormFile Photo { get; set; }

    }
}
