using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class ImportHistory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ImportType { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        public int TotalRows { get; set; }

        public int ImportedRows { get; set; }

        public int SkippedRows { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public DateTime ImportedAt { get; set; } = DateTime.Now;

        public List<ImportHistoryRow> Rows { get; set; } = new List<ImportHistoryRow>();
    }
}