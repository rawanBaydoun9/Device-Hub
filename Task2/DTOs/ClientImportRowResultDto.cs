namespace Task2.DTOs
{
    public class ClientImportRowResultDto
    {
        public int RowNumber { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}