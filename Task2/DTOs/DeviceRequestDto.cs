namespace Task2.DTOs
{
    public class DeviceRequestDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }

        public List<DeviceRequestItemDto> Items { get; set; } = new List<DeviceRequestItemDto>();
    }
}