namespace Task2.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public bool ShowOnUserHome { get; set; }
    }
}