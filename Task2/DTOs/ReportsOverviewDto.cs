namespace Task2.DTOs
{
    public class ReportsOverviewDto
    {
        public int AvailablePhoneNumbers { get; set; }

        public int ReservedPhoneNumbers { get; set; }

        public int ClientsWithoutActiveReservations { get; set; }

        public int ClientsWithActiveReservations { get; set; }

        public string MostActiveClientName { get; set; } = string.Empty;

        public int MostActiveClientReservations { get; set; }

        public string MostReservedDeviceName { get; set; } = string.Empty;

        public int MostReservedDeviceCount { get; set; }
    }
}