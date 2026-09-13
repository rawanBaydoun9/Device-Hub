using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Task2.Data;
using Task2.DTOs;
using Task2.Enums;
using Task2.Mappers;
using Task2.Models;

namespace Task2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/clients")]
    public class ClientsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/clients/GetAllClients
        [HttpGet("GetAllClients")]
        public async Task<IActionResult> GetAllClients()
        {
            var now = DateTime.Now;

            var clients = await _context.Clients
                .OrderBy(c => c.Id)
                .ToListAsync();

            var activeReservationCounts = await _context.PhoneNumberReservations
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .GroupBy(r => r.ClientId)
                .Select(g => new
                {
                    ClientId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var clientDtos = clients.Select(client =>
            {
                var activeCount = activeReservationCounts
                    .FirstOrDefault(x => x.ClientId == client.Id)?.Count ?? 0;

                return ClientMapper.ToDto(client, activeCount);
            }).ToList();

            return Ok(clientDtos);
        }

        // POST: api/clients/AddClient
        [HttpPost("AddClient")]
        public async Task<IActionResult> AddClient(Client client)
        {
            string? validationError = ValidateClient(client);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            client.Name = client.Name.Trim();

            if (client.Type == ClientType.Organization)
            {
                client.BirthDate = null;
            }

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var clientDto = ClientMapper.ToDto(client);

            return Ok(clientDto);
        }

        // PUT: api/clients/UpdateClient/5
        [HttpPut("UpdateClient/{id}")]
        public async Task<IActionResult> UpdateClient(int id, Client updatedClient)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound("Client not found");
            }

            string? validationError = ValidateClient(updatedClient);

            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            client.Name = updatedClient.Name.Trim();
            client.Type = updatedClient.Type;

            if (updatedClient.Type == ClientType.Organization)
            {
                client.BirthDate = null;
            }
            else
            {
                client.BirthDate = updatedClient.BirthDate;
            }

            await _context.SaveChangesAsync();

            var clientDto = ClientMapper.ToDto(client);

            return Ok(clientDto);
        }

        // DELETE: api/clients/DeleteClient/5
        [HttpDelete("DeleteClient/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound("Client not found");
            }

            bool hasReservations = await _context.PhoneNumberReservations
                .AnyAsync(r => r.ClientId == id);

            if (hasReservations)
            {
                return BadRequest("Cannot delete this client because they have reservation history. The history must remain.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/clients/ImportClients
        [HttpPost("ImportClients")]
        public async Task<IActionResult> ImportClients(IFormFile file)
        {
            var result = new ClientImportResultDto();

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
                ImportType = "Clients",
                FileName = file.FileName,
                ImportedAt = DateTime.Now,
                Status = "Processing"
            };

            using var stream = new StreamReader(file.OpenReadStream());

            int rowNumber = 0;

            var importedNamesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddRowResult(int currentRowNumber, string currentName, string status, string message)
            {
                result.RowResults.Add(new ClientImportRowResultDto
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

                // Skip header row
                if (rowNumber == 1 && line.ToLower().Contains("name"))
                {
                    continue;
                }

                var columns = line.Split(',');

                if (columns.Length < 2)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, "", "Skipped", "Invalid row format");
                    continue;
                }

                string name = columns[0].Trim();
                string typeText = columns[1].Trim();
                string birthDateText = columns.Length >= 3 ? columns[2].Trim() : "";

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Client name is required");
                    continue;
                }

                if (name.Length > 50)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Client name cannot exceed 50 characters");
                    continue;
                }

                bool alreadyExists = await _context.Clients
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower());

                if (alreadyExists)
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Duplicate client name");
                    continue;
                }

                if (importedNamesInFile.Contains(name))
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Duplicate client name inside the uploaded file");
                    continue;
                }

                ClientType clientType;

                if (typeText.Equals("Individual", StringComparison.OrdinalIgnoreCase))
                {
                    clientType = ClientType.Individual;
                }
                else if (typeText.Equals("Organization", StringComparison.OrdinalIgnoreCase))
                {
                    clientType = ClientType.Organization;
                }
                else
                {
                    result.SkippedRows++;
                    AddRowResult(rowNumber, name, "Skipped", "Client type must be Individual or Organization");
                    continue;
                }

                DateTime? birthDate = null;

                if (clientType == ClientType.Individual)
                {
                    if (string.IsNullOrWhiteSpace(birthDateText))
                    {
                        result.SkippedRows++;
                        AddRowResult(rowNumber, name, "Skipped", "BirthDate is required for Individual clients");
                        continue;
                    }

                    if (!DateTime.TryParse(birthDateText, out DateTime parsedBirthDate))
                    {
                        result.SkippedRows++;
                        AddRowResult(rowNumber, name, "Skipped", "Invalid BirthDate format");
                        continue;
                    }

                    if (parsedBirthDate.Date > DateTime.Today)
                    {
                        result.SkippedRows++;
                        AddRowResult(rowNumber, name, "Skipped", "BirthDate cannot be in the future");
                        continue;
                    }

                    birthDate = parsedBirthDate;
                }

                if (clientType == ClientType.Organization)
                {
                    birthDate = null;
                }

                importedNamesInFile.Add(name);

                var client = new Client
                {
                    Name = name,
                    Type = clientType,
                    BirthDate = birthDate
                };

                _context.Clients.Add(client);

                result.ImportedRows++;
                AddRowResult(rowNumber, name, "Imported", "Client imported successfully");
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

        private string? ValidateClient(Client client)
        {
            if (client == null)
            {
                return "Client data is required";
            }

            if (string.IsNullOrWhiteSpace(client.Name))
            {
                return "Client name is required";
            }

            client.Name = client.Name.Trim();

            if (client.Name.Length > 50)
            {
                return "Client name cannot exceed 50 characters";
            }

            if (!Enum.IsDefined(typeof(ClientType), client.Type))
            {
                return "Client type is required";
            }

            if (client.Type == ClientType.Individual)
            {
                if (!client.BirthDate.HasValue)
                {
                    return "Birth date is required for individual clients";
                }

                if (client.BirthDate.Value.Date > DateTime.Today)
                {
                    return "Birth date cannot be in the future";
                }
            }

            return null;
        }
    }
}