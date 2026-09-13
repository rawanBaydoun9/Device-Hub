namespace Task2.DTOs
{
    public class DeviceImportResultDto
    {
        public int TotalRows { get; set; }

        public int ImportedRows { get; set; }

        public int SkippedRows { get; set; }

        public List<DeviceImportRowResultDto> RowResults { get; set; } = new List<DeviceImportRowResultDto>();
    }
}