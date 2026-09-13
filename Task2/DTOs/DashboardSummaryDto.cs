namespace Task2.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalDevices { get; set; }

        public int TotalCategories { get; set; }

        public int TotalClients { get; set; }

        public int TotalPhoneNumbers { get; set; }

        public int ActiveReservations { get; set; }

        public int EndedReservations { get; set; }
    }
}