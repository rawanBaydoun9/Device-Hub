namespace Task2.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public string DeviceName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int AvailableQuantity { get; set; }

        public DateTime AddedAt { get; set; }
    }
}