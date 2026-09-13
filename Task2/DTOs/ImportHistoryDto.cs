namespace Task2.DTOs
{
    public class ImportHistoryDto
    {
        public int Id { get; set; }

        public string ImportType { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public int TotalRows { get; set; }

        public int ImportedRows { get; set; }

        public int SkippedRows { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ImportedAtText { get; set; } = string.Empty;
    }
}
