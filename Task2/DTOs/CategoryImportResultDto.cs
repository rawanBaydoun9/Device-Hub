namespace Task2.DTOs
{
    public class CategoryImportResultDto
    {
        public int TotalRows { get; set; }

        public int ImportedRows { get; set; }

        public int SkippedRows { get; set; }

        public List<CategoryImportRowResultDto> RowResults { get; set; } = new List<CategoryImportRowResultDto>();
    }
}