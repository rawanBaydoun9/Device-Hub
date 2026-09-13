namespace Task2.DTOs
{
    public class ImportHistoryRowDto
    {
        public int RowNumber { get; set; }

        public string RecordName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}