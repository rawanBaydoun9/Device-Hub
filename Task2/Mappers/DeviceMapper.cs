using Task2.DTOs;
using Task2.Models;

namespace Task2.Mappers
{
    public static class DeviceMapper
    {
        // Delegate:
        // A delegate defines the shape of a method.
        // Any method that receives a Device and returns a DeviceDto can match this delegate.
        public delegate DeviceDto DeviceMappingDelegate(Device device);

        // Func:
        // Func<Device, DeviceDto> means:
        // receive Device, return DeviceDto.
        public static readonly Func<Device, DeviceDto> MapDeviceFunc = device => ToDto(device);

        public static DeviceDto ToDto(Device device)
        {
            return new DeviceDto
            {
                Id = device.Id,
                Name = device.Name,
                Quantity = device.Quantity,
                CategoryId = device.CategoryId,
                CategoryName = BuildText(
                    device.Category != null ? device.Category.Name : ""
                ),
                ShowOnUserHome = device.ShowOnUserHome
            };
        }

        public static List<DeviceDto> ToDtoList(List<Device> devices)
        {
            // Using the delegate here
            DeviceMappingDelegate mapperDelegate = ToDto;

            return devices.Select(device => mapperDelegate(device)).ToList();
        }

        public static List<DeviceDto> ToDtoListUsingFunc(List<Device> devices)
        {
            // Using Func here
            return devices.Select(device => MapDeviceFunc(device)).ToList();
        }

        // params:
        // This method can receive one or many text values.
        // It joins only valid non-empty values.
        public static string BuildText(params string[] values)
        {
            return string.Join(" ",
                values.Where(value => !string.IsNullOrWhiteSpace(value))
                      .Select(value => value.Trim()));
        }
    }
}