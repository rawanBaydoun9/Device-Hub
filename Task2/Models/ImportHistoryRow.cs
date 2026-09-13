using System.ComponentModel.DataAnnotations;

namespace Task2.Models
{
    public class ImportHistoryRow
    {
        public int Id { get; set; }

        public int ImportHistoryId { get; set; }

        public ImportHistory? ImportHistory { get; set; }

        public int RowNumber { get; set; }

        [StringLength(100)]
        public string RecordName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(255)]
        public string Message { get; set; } = string.Empty;
    }
}