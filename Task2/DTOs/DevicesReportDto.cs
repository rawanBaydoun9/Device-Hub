namespace Task2.DTOs
{
    public class DevicesReportDto
    {
        public string CategoryName { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public int ReservedPhoneNumbers { get; set; }

        public int UnreservedPhoneNumbers { get; set; }

        public int TotalPhoneNumbers { get; set; }

        public string DeviceAvailability { get; set; } = string.Empty;
    }
}