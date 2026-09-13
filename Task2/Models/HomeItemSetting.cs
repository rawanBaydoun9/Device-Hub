using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class HomeItemSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string Title { get; set; } = string.Empty;

        [StringLength(150)]
        public string Description { get; set; } = string.Empty;

        [StringLength(80)]
        public string IconClass { get; set; } = string.Empty;

        [StringLength(50)]
        public string ControllerName { get; set; } = string.Empty;

        [StringLength(50)]
        public string ActionName { get; set; } = "Index";

        public bool IsVisible { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}