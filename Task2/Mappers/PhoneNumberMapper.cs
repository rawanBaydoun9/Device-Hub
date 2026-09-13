using Task2.DTOs;
using Task2.Models;

namespace Task2.Mappers
{
    public static class PhoneNumberMapper
    {
        public static PhoneNumberDto ToDto(PhoneNumber phoneNumber)
        {
            return new PhoneNumberDto
            {
                Id = phoneNumber.Id,
                Number = phoneNumber.Number,
                DeviceId = phoneNumber.DeviceId,
                DeviceName = phoneNumber.Device != null ? phoneNumber.Device.Name : ""
            };
        }

        public static List<PhoneNumberDto> ToDtoList(List<PhoneNumber> phoneNumbers)
        {
            return phoneNumbers.Select(phoneNumber => ToDto(phoneNumber)).ToList();
        }
    }
}