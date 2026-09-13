using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        public List<Device> Devices { get; set; } = new List<Device>();
    }
}