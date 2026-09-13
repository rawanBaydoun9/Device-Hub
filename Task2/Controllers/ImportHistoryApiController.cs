using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Task2.Data;
using Task2.DTOs;

namespace Task2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/importHistory")]
    public class ImportHistoryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImportHistoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/importHistory/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var histories = await _context.ImportHistories
                .OrderByDescending(h => h.ImportedAt)
                .Select(h => new ImportHistoryDto
                {
                    Id = h.Id,
                    ImportType = h.ImportType,
                    FileName = h.FileName,
                    TotalRows = h.TotalRows,
                    ImportedRows = h.ImportedRows,
                    SkippedRows = h.SkippedRows,
                    Status = h.Status,
                    ImportedAtText = h.ImportedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Ok(histories);
        }

        // GET: api/importHistory/GetDetails/5
        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var history = await _context.ImportHistories
                .Include(h => h.Rows)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (history == null)
            {
                return NotFound("Import history not found");
            }

            var rows = history.Rows
                .OrderBy(r => r.RowNumber)
                .Select(r => new ImportHistoryRowDto
                {
                    RowNumber = r.RowNumber,
                    RecordName = r.RecordName,
                    Status = r.Status,
                    Message = r.Message
                })
                .ToList();

            return Ok(rows);
        }

        // DELETE: api/importHistory/ClearAll
        [HttpDelete("ClearAll")]
        public async Task<IActionResult> ClearAll()
        {
            var histories = await _context.ImportHistories
                .Include(h => h.Rows)
                .ToListAsync();

            if (histories.Count == 0)
            {
                return Ok("No import history to clear");
            }

            _context.ImportHistories.RemoveRange(histories);

            await _context.SaveChangesAsync();

            return Ok("Import history cleared successfully");
        }
    }
}