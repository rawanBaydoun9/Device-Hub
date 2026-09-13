namespace Task2.DTOs
{
    public class ClientsReportDto
    {
        public string ClientType { get; set; } = string.Empty;

        public int NumberOfClients { get; set; }

        public int ClientsWithActiveReservations { get; set; }

        public int ClientsWithoutActiveReservations { get; set; }
    }
}