using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class DeviceRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public List<DeviceRequestItem> Items { get; set; } = new List<DeviceRequestItem>();
    }
}