namespace Task2.DTOs
{
    public class ClientImportResultDto
    {
        public int TotalRows { get; set; }

        public int ImportedRows { get; set; }

        public int SkippedRows { get; set; }

        public List<ClientImportRowResultDto> RowResults { get; set; } = new List<ClientImportRowResultDto>();
    }
}