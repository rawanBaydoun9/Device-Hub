using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Task2.Data;
using Task2.DTOs;
using Task2.Mappers;
using Task2.Models;


namespace Task2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories/GetAllCategories
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Id)
                .ToListAsync();

            var categoryDtos = CategoryMapper.ToDtoList(categories);

            return Ok(categoryDtos);
        }

        // POST: api/categories/AddCategory
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory(Category category)
        {
            string? validationError = await ValidateCategory(category);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            category.Name = category.Name.Trim();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = CategoryMapper.ToDto(category);

            return Ok(categoryDto);
        }

        // PUT: api/categories/UpdateCategory/5
        [HttpPut("UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found");
            }

            string? validationError = await ValidateCategory(updatedCategory, id);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            category.Name = updatedCategory.Name.Trim();

            await _context.SaveChangesAsync();

            var categoryDto = CategoryMapper.ToDto(category);

            return Ok(categoryDto);
        }

        [HttpDelete("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found");
            }

            bool hasDevices = await _context.Devices
                .AnyAsync(d => d.CategoryId == id);

            if (hasDevices)
            {
                return BadRequest("Cannot delete this category because it has devices assigned to it. Move or delete the devices first.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string?> ValidateCategory(Category category, int? editingId = null)
        {
            if (category == null)
            {
                return "Category data is required";
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return "Category name is required";
            }

            category.Name = category.Name.Trim();

            if (category.Name.Length > 50)
            {
                return "Category name cannot exceed 50 characters";
            }

            bool nameExists = await _context.Categories.AnyAsync(c =>
                c.Name.ToLower() == category.Name.ToLower()
                && (!editingId.HasValue || c.Id != editingId.Value));

            if (nameExists)
            {
                return "Category name already exists";
            }

            return null;
        }

        // POST: api/categories/ImportCategories
        [HttpPost("ImportCategories")]
        public async Task<IActionResult> ImportCategories(IFormFile file)
        {
            var result = new CategoryImportResultDto();

            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid CSV file");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CSV files are allowed");
            }

            var importHistory = new ImportHistory
            {
                ImportType = "Categories",
                FileName = file.FileName,
                ImportedAt = DateTime.Now,
                Status = "Processing"
            };

            using var stream = new StreamReader(file.OpenReadStream());

            int rowNumber = 0;

            var importedNamesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddRowResult(int currentRowNumber, string currentName, string status, string message)
            {
                result.RowResults.Add(new CategoryImportRowResultDto
                {
                    RowNumber = currentRowNumber,
                    Name = currentName,
                    Status = status,
                    Message = message
                });

                importHistory.Rows.Add(new ImportHistoryRow
                {
                    RowNumber = currentRowNumber,
                    RecordName = currentName,
                    Status = status,
                    Message = message
                });
            }

            while (!stream.EndOfStream)
            {
                var line = await stream.ReadLineAsync();
                rowNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var columns = line.Split(',');

                // Header validation
                if (rowNumber == 1)
                {
                    var firstColumn = columns[0].Trim();

                    if (firstColumn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                if (columns.Length > 1)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, columns[0].Trim(), "Skipped", "Categories CSV must contain only one column: Name");
                    continue;
                }

                string name = columns[0].Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Category name is required");
                    continue;
                }

                if (name.Length > 50)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Category name cannot exceed 50 characters");
                    continue;
                }

                bool alreadyExists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower());

                if (alreadyExists)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Duplicate category name");
                    continue;
                }

                if (importedNamesInFile.Contains(name))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Duplicate category name inside the uploaded file");
                    continue;
                }

                importedNamesInFile.Add(name);

                var category = new Category
                {
                    Name = name
                };

                _context.Categories.Add(category);

                result.ImportedRows++;
                AddRowResult(rowNumber, name, "Imported", "Category imported successfully");
            }

            result.TotalRows = importHistory.Rows.Count;

            importHistory.TotalRows = result.TotalRows;
            importHistory.ImportedRows = result.ImportedRows;
            importHistory.SkippedRows = result.SkippedRows;

            if (result.ImportedRows > 0 && result.SkippedRows == 0)
            {
                importHistory.Status = "Completed";
            }
            else if (result.ImportedRows > 0 && result.SkippedRows > 0)
            {
                importHistory.Status = "Completed with Errors";
            }
            else
            {
                importHistory.Status = "Failed";
            }

            _context.ImportHistories.Add(importHistory);

            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }
}