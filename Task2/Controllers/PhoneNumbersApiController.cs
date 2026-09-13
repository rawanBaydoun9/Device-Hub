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
    [Route("api/phoneNumbers")]
    public class PhoneNumbersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PhoneNumbersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/phoneNumbers/GetAllPhoneNumbers
        [HttpGet("GetAllPhoneNumbers")]
        public async Task<IActionResult> GetAllPhoneNumbers()
        {
            var phoneNumbers = await _context.PhoneNumbers
                .Include(p => p.Device)
                .OrderBy(p => p.Id)
                .ToListAsync();

            var phoneNumberDtos = PhoneNumberMapper.ToDtoList(phoneNumbers);

            return Ok(phoneNumberDtos);
        }

        // POST: api/phoneNumbers/AddPhoneNumber
        [HttpPost("AddPhoneNumber")]
        public async Task<IActionResult> AddPhoneNumber(PhoneNumber phoneNumber)
        {
            string? validationError = await ValidatePhoneNumber(phoneNumber);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            phoneNumber.Number = phoneNumber.Number.Trim();

            _context.PhoneNumbers.Add(phoneNumber);
            await _context.SaveChangesAsync();

            var savedPhoneNumber = await _context.PhoneNumbers
                .Include(p => p.Device)
                .FirstOrDefaultAsync(p => p.Id == phoneNumber.Id);

            if (savedPhoneNumber == null)
            {
                return NotFound("Phone number not found after saving");
            }

            var phoneNumberDto = PhoneNumberMapper.ToDto(savedPhoneNumber);

            return Ok(phoneNumberDto);
        }

        // PUT: api/phoneNumbers/UpdatePhoneNumber/5
        [HttpPut("UpdatePhoneNumber/{id}")]
        public async Task<IActionResult> UpdatePhoneNumber(int id, PhoneNumber updatedPhoneNumber)
        {
            var phoneNumber = await _context.PhoneNumbers.FindAsync(id);

            if (phoneNumber == null)
            {
                return NotFound("Phone number not found");
            }

            string? validationError = await ValidatePhoneNumber(updatedPhoneNumber, id);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            phoneNumber.Number = updatedPhoneNumber.Number.Trim();
            phoneNumber.DeviceId = updatedPhoneNumber.DeviceId;

            await _context.SaveChangesAsync();

            var savedPhoneNumber = await _context.PhoneNumbers
                .Include(p => p.Device)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (savedPhoneNumber == null)
            {
                return NotFound("Phone number not found after updating");
            }

            var phoneNumberDto = PhoneNumberMapper.ToDto(savedPhoneNumber);

            return Ok(phoneNumberDto);
        }

        // DELETE: api/phoneNumbers/DeletePhoneNumber/5
        [HttpDelete("DeletePhoneNumber/{id}")]
        public async Task<IActionResult> DeletePhoneNumber(int id)
        {
            var phoneNumber = await _context.PhoneNumbers.FindAsync(id);

            if (phoneNumber == null)
            {
                return NotFound("Phone number not found");
            }

            bool hasReservations = await _context.PhoneNumberReservations
                .AnyAsync(r => r.PhoneNumberId == id);

            if (hasReservations)
            {
                return BadRequest("Cannot delete this phone number because it has reservation history. You can unreserve it, but the history must remain.");
            }

            _context.PhoneNumbers.Remove(phoneNumber);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string?> ValidatePhoneNumber(PhoneNumber phoneNumber, int? editingId = null)
        {
            if (phoneNumber == null)
            {
                return "Phone number data is required";
            }

            if (string.IsNullOrWhiteSpace(phoneNumber.Number))
            {
                return "Phone number is required";
            }

            phoneNumber.Number = phoneNumber.Number.Trim();

            if (phoneNumber.Number.Length > 20)
            {
                return "Phone number cannot exceed 20 characters";
            }

            if (!IsValidPhoneNumber(phoneNumber.Number))
            {
                return "Phone number can contain only digits, spaces, +, -, and parentheses";
            }

            if (phoneNumber.DeviceId <= 0)
            {
                return "Device is required";
            }

            bool deviceExists = await _context.Devices
                .AnyAsync(d => d.Id == phoneNumber.DeviceId);

            if (!deviceExists)
            {
                return "Selected device does not exist";
            }

            bool numberExists = await _context.PhoneNumbers.AnyAsync(p =>
                p.Number.ToLower() == phoneNumber.Number.ToLower()
                && (!editingId.HasValue || p.Id != editingId.Value));

            if (numberExists)
            {
                return "Phone number already exists";
            }

            return null;
        }

        private bool IsValidPhoneNumber(string number)
        {
            foreach (char character in number)
            {
                bool isAllowed =
                    char.IsDigit(character) ||
                    character == '+' ||
                    character == '-' ||
                    character == '(' ||
                    character == ')' ||
                    character == ' ';

                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }

        // POST: api/phoneNumbers/ImportPhoneNumbers
        [HttpPost("ImportPhoneNumbers")]
        public async Task<IActionResult> ImportPhoneNumbers(IFormFile file)
        {
            var result = new PhoneNumberImportResultDto();

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
                ImportType = "Phone Numbers",
                FileName = file.FileName,
                ImportedAt = DateTime.Now,
                Status = "Processing"
            };

            using var stream = new StreamReader(file.OpenReadStream());

            int rowNumber = 0;

            var importedNumbersInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddRowResult(int currentRowNumber, string currentNumber, string status, string message)
            {
                result.RowResults.Add(new PhoneNumberImportRowResultDto
                {
                    RowNumber = currentRowNumber,
                    Number = currentNumber,
                    Status = status,
                    Message = message
                });

                importHistory.Rows.Add(new ImportHistoryRow
                {
                    RowNumber = currentRowNumber,
                    RecordName = currentNumber,
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
                        columns.Length >= 2 &&
                        columns[0].Trim().Equals("Number", StringComparison.OrdinalIgnoreCase) &&
                        columns[1].Trim().Equals("DeviceName", StringComparison.OrdinalIgnoreCase);

                    if (isHeader)
                    {
                        continue;
                    }
                }

                if (columns.Length != 2)
                {
                    result.SkippedRows++;
                    AddRowResult(
                        rowNumber,
                        columns.Length > 0 ? columns[0].Trim() : "",
                        "Skipped",
                        "Phone Numbers CSV must contain exactly 2 columns: Number, DeviceName"
                    );
                    continue;
                }

                string number = columns[0].Trim();
                string deviceName = columns[1].Trim();

                if (string.IsNullOrWhiteSpace(number))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Phone number is required");
                    continue;
                }

                if (number.Length > 20)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Phone number cannot exceed 20 characters");
                    continue;
                }

                bool validPhoneNumber = number.All(character =>
                    char.IsDigit(character) ||
                    character == '+' ||
                    character == '-' ||
                    character == '(' ||
                    character == ')' ||
                    character == ' '
                );

                if (!validPhoneNumber)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Phone number can contain only digits, spaces, +, -, and parentheses");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Device name is required");
                    continue;
                }

                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == deviceName.ToLower());

                if (device == null)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Device not found");
                    continue;
                }

                bool alreadyExists = await _context.PhoneNumbers
                    .AnyAsync(p => p.Number.ToLower() == number.ToLower());

                if (alreadyExists)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Duplicate phone number");
                    continue;
                }

                if (importedNumbersInFile.Contains(number))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, number, "Skipped", "Duplicate phone number inside the uploaded file");
                    continue;
                }

                importedNumbersInFile.Add(number);

                var phoneNumber = new PhoneNumber
                {
                    Number = number,
                    DeviceId = device.Id
                };

                _context.PhoneNumbers.Add(phoneNumber);

                result.ImportedRows++;
                AddRowResult(rowNumber, number, "Imported", "Phone number imported successfully");
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