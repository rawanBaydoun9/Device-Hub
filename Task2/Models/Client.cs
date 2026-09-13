using System.ComponentModel.DataAnnotations;
using Task2.Enums;

namespace Task2.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required")]
        [StringLength(50, ErrorMessage = "Client name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client type is required")]
        public ClientType Type { get; set; }

        public DateTime? BirthDate { get; set; }

        public List<PhoneNumberReservation> PhoneNumberReservations { get; set; } = new List<PhoneNumberReservation>();
    }
}