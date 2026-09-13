using Task2.DTOs;
using Task2.Models;

namespace Task2.Mappers
{
    public static class PhoneNumberReservationMapper
    {
        public static PhoneNumberReservationDto ToDto(PhoneNumberReservation reservation)
        {
            var now = DateTime.Now;

            bool isActive =
                reservation.BED <= now &&
                (!reservation.EED.HasValue || reservation.EED.Value > now);

            return new PhoneNumberReservationDto
            {
                Id = reservation.Id,

                ClientId = reservation.ClientId,
                ClientName = reservation.Client != null ? reservation.Client.Name : "",

                PhoneNumberId = reservation.PhoneNumberId,
                PhoneNumberValue = reservation.PhoneNumber != null ? reservation.PhoneNumber.Number : "",

                BED = reservation.BED,
                BEDText = reservation.BED.ToString("yyyy-MM-dd"),

                EED = reservation.EED,
                EEDText = reservation.EED.HasValue
                    ? reservation.EED.Value.ToString("yyyy-MM-dd")
                    : "Active",

                IsActive = isActive,
                StatusText = isActive ? "Active" : "Ended"
            };
        }

        public static List<PhoneNumberReservationDto> ToDtoList(List<PhoneNumberReservation> reservations)
        {
            return reservations.Select(reservation => ToDto(reservation)).ToList();
        }
    }
}