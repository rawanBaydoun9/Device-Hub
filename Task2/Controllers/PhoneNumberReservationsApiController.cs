using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Task2.Data;
using Task2.DTOs;
using Task2.Mappers;
using Task2.Models;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace Task2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/phoneNumberReservations")]
    public class PhoneNumberReservationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PhoneNumberReservationsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/phoneNumberReservations/GetAllReservations
        [HttpGet("GetAllReservations")]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _context.PhoneNumberReservations
                .Include(r => r.Client)
                .Include(r => r.PhoneNumber)
                .OrderBy(r => r.Id)
                .ToListAsync();

            var reservationDtos = PhoneNumberReservationMapper.ToDtoList(reservations);

            return Ok(reservationDtos);
        }

        // GET: api/phoneNumberReservations/GetFilteredReservations
        [HttpGet("GetFilteredReservations")]
public async Task<IActionResult> GetFilteredReservations(
    int? clientId = null,
    int? phoneNumberId = null,
    string? status = null)
{
    var now = DateTime.Now;

    var query = _context.PhoneNumberReservations
        .Include(r => r.Client)
        .Include(r => r.PhoneNumber)
        .AsQueryable();

    if (clientId.HasValue && clientId.Value > 0)
    {
        query = query.Where(r => r.ClientId == clientId.Value);
    }

    if (phoneNumberId.HasValue && phoneNumberId.Value > 0)
    {
        query = query.Where(r => r.PhoneNumberId == phoneNumberId.Value);
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        status = status.Trim().ToLower();

        if (status == "active")
        {
            query = query.Where(r =>
                r.BED <= now &&
                (!r.EED.HasValue || r.EED.Value > now));
        }
                else if (status == "ended")
                {
                    query = query.Where(r =>
                        r.EED.HasValue &&
                        r.EED.Value <= now);
                }
            }

    var reservations = await query
        .OrderByDescending(r => r.BED)
        .ThenByDescending(r => r.Id)
        .ToListAsync();

    var reservationDtos = PhoneNumberReservationMapper.ToDtoList(reservations);

    return Ok(reservationDtos);
}

        // DELETE: api/phoneNumberReservations/DeleteReservation/5
        // Optional: useful if you want delete only, but no Add/Edit.
        [HttpDelete("DeleteReservation/{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.PhoneNumberReservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound("Reservation not found");
            }

            _context.PhoneNumberReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/phoneNumberReservations/GetAvailablePhoneNumbers
        [HttpGet("GetAvailablePhoneNumbers")]
        public async Task<IActionResult> GetAvailablePhoneNumbers()
        {
            var now = DateTime.Now;

            var activeReservedPhoneNumberIds = await _context.PhoneNumberReservations
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .Select(r => r.PhoneNumberId)
                .ToListAsync();

            var availablePhoneNumbers = await _context.PhoneNumbers
                .Include(p => p.Device)
                .Where(p => !activeReservedPhoneNumberIds.Contains(p.Id))
                .OrderBy(p => p.Number)
                .ToListAsync();

            var phoneNumberDtos = PhoneNumberMapper.ToDtoList(availablePhoneNumbers);

            return Ok(phoneNumberDtos);
        }

        // GET: api/phoneNumberReservations/GetClientActivePhoneNumbers/5
        [HttpGet("GetClientActivePhoneNumbers/{clientId}")]
        public async Task<IActionResult> GetClientActivePhoneNumbers(int clientId)
        {
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == clientId);

            if (!clientExists)
            {
                return NotFound("Client not found");
            }

            var now = DateTime.Now;

            var phoneNumbers = await _context.PhoneNumberReservations
                .Include(r => r.PhoneNumber)
                    .ThenInclude(p => p!.Device)
                .Where(r =>
                    r.ClientId == clientId &&
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .Select(r => r.PhoneNumber!)
                .OrderBy(p => p.Number)
                .ToListAsync();

            var phoneNumberDtos = PhoneNumberMapper.ToDtoList(phoneNumbers);

            return Ok(phoneNumberDtos);
        }

        // POST: api/phoneNumberReservations/ReservePhoneNumber
        [HttpPost("ReservePhoneNumber")]
        public async Task<IActionResult> ReservePhoneNumber(ReservePhoneNumberRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Reservation data is required");
            }

            if (request.ClientId <= 0)
            {
                return BadRequest("Client is required");
            }

            if (request.PhoneNumberId <= 0)
            {
                return BadRequest("Phone number is required");
            }

            var clientExists = await _context.Clients
                .AnyAsync(c => c.Id == request.ClientId);

            if (!clientExists)
            {
                return BadRequest("Selected client does not exist");
            }

            var phoneNumberExists = await _context.PhoneNumbers
                .AnyAsync(p => p.Id == request.PhoneNumberId);

            if (!phoneNumberExists)
            {
                return BadRequest("Selected phone number does not exist");
            }

            var now = DateTime.Now;

            bool phoneNumberIsAlreadyReserved = await _context.PhoneNumberReservations
                .AnyAsync(r =>
                    r.PhoneNumberId == request.PhoneNumberId &&
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now));

            if (phoneNumberIsAlreadyReserved)
            {
                return BadRequest("This phone number is already actively reserved");
            }

            var reservation = new PhoneNumberReservation
            {
                ClientId = request.ClientId,
                PhoneNumberId = request.PhoneNumberId,
                BED = now,
                EED = null
            };

            _context.PhoneNumberReservations.Add(reservation);
            await _context.SaveChangesAsync();

            var savedReservation = await _context.PhoneNumberReservations
                .Include(r => r.Client)
                .Include(r => r.PhoneNumber)
                .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            if (savedReservation == null)
            {
                return NotFound("Reservation not found after saving");
            }

            var reservationDto = PhoneNumberReservationMapper.ToDto(savedReservation);

            return Ok(reservationDto);
        }

        // POST: api/phoneNumberReservations/UnreservePhoneNumber
        [HttpPost("UnreservePhoneNumber")]
        public async Task<IActionResult> UnreservePhoneNumber(UnreservePhoneNumberRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Unreservation data is required");
            }

            if (request.ClientId <= 0)
            {
                return BadRequest("Client is required");
            }

            if (request.PhoneNumberId <= 0)
            {
                return BadRequest("Phone number is required");
            }

            var now = DateTime.Now;

            var activeReservation = await _context.PhoneNumberReservations
                .Where(r =>
                    r.ClientId == request.ClientId &&
                    r.PhoneNumberId == request.PhoneNumberId &&
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .OrderByDescending(r => r.BED)
                .FirstOrDefaultAsync();

            if (activeReservation == null)
            {
                return BadRequest("No active reservation found for this client and phone number");
            }

            activeReservation.EED = now;

            await _context.SaveChangesAsync();

            var savedReservation = await _context.PhoneNumberReservations
                .Include(r => r.Client)
                .Include(r => r.PhoneNumber)
                .FirstOrDefaultAsync(r => r.Id == activeReservation.Id);

            if (savedReservation == null)
            {
                return NotFound("Reservation not found after unreserving");
            }

            var reservationDto = PhoneNumberReservationMapper.ToDto(savedReservation);

            return Ok(reservationDto);
        }

        // GET: api/phoneNumberReservations/ExportReservationsWord
        [HttpGet("ExportReservationsWord")]
        public async Task<IActionResult> ExportReservationsWord(
    int? clientId = null,
    int? phoneNumberId = null,
    string? status = null)
        {
            var now = DateTime.Now;

            var query = _context.PhoneNumberReservations
                .Include(r => r.Client)
                .Include(r => r.PhoneNumber)
                .AsQueryable();

            if (clientId.HasValue && clientId.Value > 0)
            {
                query = query.Where(r => r.ClientId == clientId.Value);
            }

            if (phoneNumberId.HasValue && phoneNumberId.Value > 0)
            {
                query = query.Where(r => r.PhoneNumberId == phoneNumberId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.ToLower();

                if (status == "active")
                {
                    query = query.Where(r =>
                        r.BED <= now &&
                        (!r.EED.HasValue || r.EED.Value > now));
                }
                else if (status == "ended")
                {
                    query = query.Where(r =>
                        r.BED > now ||
                        (r.EED.HasValue && r.EED.Value <= now));
                }
            }

            var reservations = await query
                .OrderByDescending(r => r.BED)
                .ThenBy(r => r.Id)
                .ToListAsync();

            var html = new StringBuilder();

            html.Append(@"
        <html>
        <head>
            <meta charset='utf-8'>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    color: #222;
                }

                h1 {
                    color: #3f5efb;
                    margin-bottom: 5px;
                }

                .subtitle {
                    color: #666;
                    margin-bottom: 25px;
                }

                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 15px;
                }

                th {
                    background-color: #5B7CFA;
                    color: white;
                    padding: 10px;
                    border: 1px solid #ddd;
                    text-align: left;
                }

                td {
                    padding: 9px;
                    border: 1px solid #ddd;
                }

                tr:nth-child(even) {
                    background-color: #f5f7ff;
                }

                .active {
                    color: #198754;
                    font-weight: bold;
                }

                .ended {
                    color: #dc3545;
                    font-weight: bold;
                }

                .footer {
                    margin-top: 25px;
                    font-size: 12px;
                    color: #777;
                }
            </style>
        </head>
        <body>
    ");

            html.Append("<h1>Phone Number Reservations Report</h1>");
            html.Append("<p class='subtitle'>Generated on " + now.ToString("yyyy-MM-dd HH:mm") + "</p>");

            html.Append("<table>");
            html.Append(@"
        <tr>
            <th>ID</th>
            <th>Client</th>
            <th>Phone Number</th>
            <th>BED</th>
            <th>EED</th>
            <th>Status</th>
        </tr>
    ");

            foreach (var reservation in reservations)
            {
                bool isActive =
                    reservation.BED <= now &&
                    (!reservation.EED.HasValue || reservation.EED.Value > now);

                string statusText = isActive ? "Active" : "Ended";
                string statusClass = isActive ? "active" : "ended";

                html.Append("<tr>");
                html.Append("<td>" + reservation.Id + "</td>");
                html.Append("<td>" + (reservation.Client != null ? reservation.Client.Name : "") + "</td>");
                html.Append("<td>" + (reservation.PhoneNumber != null ? reservation.PhoneNumber.Number : "") + "</td>");
                html.Append("<td>" + reservation.BED.ToString("yyyy-MM-dd") + "</td>");
                html.Append("<td>" + (reservation.EED.HasValue ? reservation.EED.Value.ToString("yyyy-MM-dd") : "Active") + "</td>");
                html.Append("<td class='" + statusClass + "'>" + statusText + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");

            html.Append("<p class='footer'>Total Reservations: " + reservations.Count + "</p>");

            html.Append(@"
        </body>
        </html>
    ");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());

            var fileName = "Reservations_Report_" + now.ToString("yyyyMMdd_HHmm") + ".doc";

            return File(
                bytes,
                "application/msword",
                fileName
            );
        }
    }
}