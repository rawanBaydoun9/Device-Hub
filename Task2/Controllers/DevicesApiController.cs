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
    [Route("api/devices")]
    public class DevicesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevicesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/devices/GetAllDevices
        [HttpGet("GetAllDevices")]
        public async Task<IActionResult> GetAllDevices()
        {
            var devices = await _context.Devices
                .Include(d => d.Category)
                .OrderBy(d => d.Id)
                .ToListAsync();

            var deviceDtos = DeviceMapper.ToDtoList(devices);

            return Ok(deviceDtos);
        }

        // GET: api/devices/GetFilteredDevices?search=iphone&stockFilter=inStock&categoryId=2&pageNumber=1&pageSize=20&sortColumn=name&sortDirection=asc
        [HttpGet("GetFilteredDevices")]
        public async Task<IActionResult> GetFilteredDevices(
            string? search = "",
            string? stockFilter = "all",
            int? categoryId = null,
            int pageNumber = 1,
            int pageSize = 20,
            string? sortColumn = "id",
            string? sortDirection = "asc")
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 20;
            }

            sortColumn = string.IsNullOrWhiteSpace(sortColumn)
                ? "id"
                : sortColumn.Trim().ToLower();

            sortDirection = string.IsNullOrWhiteSpace(sortDirection)
                ? "asc"
                : sortDirection.Trim().ToLower();

            var query = _context.Devices
                .Include(d => d.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(d =>
                    d.Name.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(stockFilter))
            {
                stockFilter = stockFilter.Trim().ToLower();

                if (stockFilter == "instock")
                {
                    query = query.Where(d => d.Quantity > 0);
                }
                else if (stockFilter == "outofstock")
                {
                    query = query.Where(d => d.Quantity == 0);
                }
                else if (stockFilter == "lowstock")
                {
                    query = query.Where(d => d.Quantity > 0 && d.Quantity <= 5);
                }
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            if (sortColumn == "name")
            {
                query = sortDirection == "desc"
                    ? query.OrderByDescending(d => d.Name)
                    : query.OrderBy(d => d.Name);
            }
            else if (sortColumn == "category")
            {
                query = sortDirection == "desc"
                    ? query.OrderByDescending(d => d.Category!.Name)
                    : query.OrderBy(d => d.Category!.Name);
            }
            else if (sortColumn == "quantity")
            {
                query = sortDirection == "desc"
                    ? query.OrderByDescending(d => d.Quantity)
                    : query.OrderBy(d => d.Quantity);
            }
            else
            {
                query = sortDirection == "desc"
                    ? query.OrderByDescending(d => d.Id)
                    : query.OrderBy(d => d.Id);
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var devices = await query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

            var deviceDtos = DeviceMapper.ToDtoList(devices);

            return Ok(new
            {
                items = deviceDtos,
                totalCount = totalCount,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = totalPages,
                sortColumn = sortColumn,
                sortDirection = sortDirection
            });
        }

        // POST: api/devices/AddDevice
        [HttpPost("AddDevice")]
        public async Task<IActionResult> AddDevice(Device device)
        {
            string? validationError = await ValidateDevice(device);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            device.Name = device.Name.Trim();

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var savedDevice = await _context.Devices
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == device.Id);

            if (savedDevice == null)
            {
                return NotFound("Device not found after saving");
            }

            var deviceDto = DeviceMapper.ToDto(savedDevice);

            return Ok(deviceDto);
        }

        // PUT: api/devices/UpdateDevice/5
        [HttpPut("UpdateDevice/{id}")]
        public async Task<IActionResult> UpdateDevice(int id, Device updatedDevice)
        {
            var device = await _context.Devices.FindAsync(id);

            if (device == null)
            {
                return NotFound("Device not found");
            }

            string? validationError = await ValidateDevice(updatedDevice, id);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            device.Name = updatedDevice.Name.Trim();
            device.Quantity = updatedDevice.Quantity;
            device.CategoryId = updatedDevice.CategoryId;

            await _context.SaveChangesAsync();

            var savedDevice = await _context.Devices
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (savedDevice == null)
            {
                return NotFound("Device not found after updating");
            }

            var deviceDto = DeviceMapper.ToDto(savedDevice);

            return Ok(deviceDto);
        }

        // DELETE: api/devices/DeleteDevice/5
        [HttpDelete("DeleteDevice/{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);

            if (device == null)
            {
                return NotFound("Device not found");
            }

            bool deviceHasPhoneNumbers = await _context.PhoneNumbers
                .AnyAsync(p => p.DeviceId == id);

            if (deviceHasPhoneNumbers)
            {
                return BadRequest("Cannot delete this device because it has assigned phone numbers");
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/devices/ToggleUserHomeVisibility/5
        [Authorize(Roles = "Admin")]
        [HttpPost("ToggleUserHomeVisibility/{id}")]
        public async Task<IActionResult> ToggleUserHomeVisibility(int id)
        {
            var device = await _context.Devices.FindAsync(id);

            if (device == null)
            {
                return NotFound("Device not found");
            }

            device.ShowOnUserHome = !device.ShowOnUserHome;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = device.ShowOnUserHome
                    ? "Device is now visible on user homepage."
                    : "Device is now hidden from user homepage.",
                showOnUserHome = device.ShowOnUserHome
            });
        }

        // GET: api/devices/GetUserHomeDevices
        [HttpGet("GetUserHomeDevices")]
        public async Task<IActionResult> GetUserHomeDevices()
        {
            var devices = await _context.Devices
                .Include(d => d.Category)
                .Where(d => d.ShowOnUserHome && d.Quantity > 0)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var deviceDtos = DeviceMapper.ToDtoList(devices);

            return Ok(deviceDtos);
        }

        private async Task<string?> ValidateDevice(Device device, int? editingId = null)
        {
            if (device == null)
            {
                return "Device data is required";
            }

            if (string.IsNullOrWhiteSpace(device.Name))
            {
                return "Device name is required";
            }

            device.Name = device.Name.Trim();

            if (device.Name.Length > 50)
            {
                return "Device name cannot exceed 50 characters";
            }

            if (device.Quantity < 0)
            {
                return "Quantity cannot be negative";
            }

            if (device.CategoryId <= 0)
            {
                return "Category is required";
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == device.CategoryId);

            if (!categoryExists)
            {
                return "Selected category does not exist";
            }

            bool nameExists = await _context.Devices.AnyAsync(d =>
                d.Name.ToLower() == device.Name.ToLower()
                && (!editingId.HasValue || d.Id != editingId.Value));

            if (nameExists)
            {
                return "Device name already exists";
            }

            return null;
        }
        // POST: api/devices/ImportDevices
        [HttpPost("ImportDevices")]
        public async Task<IActionResult> ImportDevices(IFormFile file)
        {
            var result = new DeviceImportResultDto();

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
                ImportType = "Devices",
                FileName = file.FileName,
                ImportedAt = DateTime.Now,
                Status = "Processing"
            };

            using var stream = new StreamReader(file.OpenReadStream());

            int rowNumber = 0;

            var importedNamesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddRowResult(int currentRowNumber, string currentName, string status, string message)
            {
                result.RowResults.Add(new DeviceImportRowResultDto
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

                // Skip header row
                if (rowNumber == 1)
                {
                    bool isHeader =
                        columns.Length >= 3 &&
                        columns[0].Trim().Equals("Name", StringComparison.OrdinalIgnoreCase) &&
                        columns[1].Trim().Equals("Quantity", StringComparison.OrdinalIgnoreCase) &&
                        columns[2].Trim().Equals("CategoryName", StringComparison.OrdinalIgnoreCase);

                    if (isHeader)
                    {
                        continue;
                    }
                }

                if (columns.Length != 3)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, columns.Length > 0 ? columns[0].Trim() : "", "Skipped", "Devices CSV must contain exactly 3 columns: Name, Quantity, CategoryName");
                    continue;
                }

                string name = columns[0].Trim();
                string quantityText = columns[1].Trim();
                string categoryName = columns[2].Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Device name is required");
                    continue;
                }

                if (name.Length > 50)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Device name cannot exceed 50 characters");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(quantityText))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Quantity is required");
                    continue;
                }

                if (!int.TryParse(quantityText, out int quantity))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Quantity must be a valid number");
                    continue;
                }

                if (quantity < 0)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Quantity cannot be negative");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Category name is required");
                    continue;
                }

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

                if (category == null)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Category not found");
                    continue;
                }

                bool alreadyExists = await _context.Devices
                    .AnyAsync(d => d.Name.ToLower() == name.ToLower());

                if (alreadyExists)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Duplicate device name");
                    continue;
                }

                if (importedNamesInFile.Contains(name))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Duplicate device name inside the uploaded file");
                    continue;
                }

                importedNamesInFile.Add(name);

                var device = new Device
                {
                    Name = name,
                    Quantity = quantity,
                    CategoryId = category.Id
                };

                _context.Devices.Add(device);

                result.ImportedRows++;
                AddRowResult(rowNumber, name, "Imported", "Device imported successfully");
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