using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class PhoneNumberReservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client is required")]
        public int ClientId { get; set; }

        public Client? Client { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public int PhoneNumberId { get; set; }

        public PhoneNumber? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Begin effective date is required")]
        public DateTime BED { get; set; }

        public DateTime? EED { get; set; }
    }
}