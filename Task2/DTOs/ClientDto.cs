using Task2.Enums;

namespace Task2.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ClientType Type { get; set; }

        public string TypeName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public string BirthDateText { get; set; } = string.Empty;

        public int ActiveReservationCount { get; set; }

        public bool HasActivePhoneReservation { get; set; }
    }
}