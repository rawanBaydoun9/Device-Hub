using Task2.DTOs;
using Task2.Models;

namespace Task2.Mappers
{
    public static class ClientMapper
    {
        public static ClientDto ToDto(Client client, int activeReservationCount = 0)
        {
            return new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                Type = client.Type,
                TypeName = client.Type.ToString(),
                BirthDate = client.BirthDate,
                BirthDateText = client.BirthDate.HasValue
                    ? client.BirthDate.Value.ToString("yyyy-MM-dd")
                    : "-",
                ActiveReservationCount = activeReservationCount,
                HasActivePhoneReservation = activeReservationCount > 0
            };
        }

        public static List<ClientDto> ToDtoList(List<Client> clients)
        {
            return clients.Select(client => ToDto(client)).ToList();
        }
    }
}