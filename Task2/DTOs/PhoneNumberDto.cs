namespace Task2.DTOs
{
    public class PhoneNumberDto
    {
        public int Id { get; set; }

        public string Number { get; set; } = string.Empty;

        public int DeviceId { get; set; }

        public string DeviceName { get; set; } = string.Empty;
    }
}