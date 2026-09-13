namespace Task2.DTOs
{
    public class PhoneNumberImportResultDto
    {
        public int TotalRows { get; set; }

        public int ImportedRows { get; set; }

        public int SkippedRows { get; set; }

        public List<PhoneNumberImportRowResultDto> RowResults { get; set; } = new List<PhoneNumberImportRowResultDto>();
    }
}