namespace Task2.DTOs
{
    public class PhoneNumberReservationDto
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public int PhoneNumberId { get; set; }

        public string PhoneNumberValue { get; set; } = string.Empty;

        public DateTime BED { get; set; }

        public string BEDText { get; set; } = string.Empty;

        public DateTime? EED { get; set; }

        public string EEDText { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string StatusText { get; set; } = string.Empty;
    }
}