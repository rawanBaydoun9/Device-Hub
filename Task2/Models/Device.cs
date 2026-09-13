using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Device name is required")]
        [StringLength(50, ErrorMessage = "Device name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int Quantity { get; set; } = 1;
        public bool ShowOnUserHome { get; set; } = false;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public List<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
    }
}