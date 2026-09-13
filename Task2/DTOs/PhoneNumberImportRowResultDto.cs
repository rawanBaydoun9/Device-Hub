namespace Task2.DTOs
{
    public class PhoneNumberImportRowResultDto
    {
        public int RowNumber { get; set; }

        public string Number { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}