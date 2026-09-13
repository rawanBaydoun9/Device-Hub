using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class PhoneNumber
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Device is required")]
        public int DeviceId { get; set; }

        public Device? Device { get; set; }
        public List<PhoneNumberReservation> PhoneNumberReservations { get; set; } = new List<PhoneNumberReservation>();
    }
}