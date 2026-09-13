namespace Task2.Models
{
    public class DeviceRequestItem
    {
        public int Id { get; set; }

        public int DeviceRequestId { get; set; }

        public DeviceRequest? DeviceRequest { get; set; }

        public int DeviceId { get; set; }

        public Device? Device { get; set; }

        public int Quantity { get; set; }
    }
}