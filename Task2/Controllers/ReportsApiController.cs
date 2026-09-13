using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Task2.Data;
using Task2.DTOs;
using Task2.Enums;

namespace Task2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/reports")]
    public class ReportsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reports/GetClientsReport
        [HttpGet("GetClientsReport")]
        public async Task<IActionResult> GetClientsReport(
            int? clientType = null,
            string? reservationActivity = null)
        {
            var now = DateTime.Now;

            var clientsQuery = _context.Clients.AsQueryable();

            if (clientType.HasValue && clientType.Value > 0)
            {
                clientsQuery = clientsQuery.Where(c => (int)c.Type == clientType.Value);
            }

            var clients = await clientsQuery.ToListAsync();

            var activeClientIds = await _context.PhoneNumberReservations
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .Select(r => r.ClientId)
                .Distinct()
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(reservationActivity))
            {
                reservationActivity = reservationActivity.Trim().ToLower();

                if (reservationActivity == "with-active")
                {
                    clients = clients
                        .Where(c => activeClientIds.Contains(c.Id))
                        .ToList();
                }
                else if (reservationActivity == "without-active")
                {
                    clients = clients
                        .Where(c => !activeClientIds.Contains(c.Id))
                        .ToList();
                }
            }

            var report = clients
                .GroupBy(c => c.Type)
                .Select(group => new ClientsReportDto
                {
                    ClientType = group.Key.ToString(),

                    NumberOfClients = group.Count(),

                    ClientsWithActiveReservations = group
                        .Count(c => activeClientIds.Contains(c.Id)),

                    ClientsWithoutActiveReservations = group
                        .Count(c => !activeClientIds.Contains(c.Id))
                })
                .OrderBy(r => r.ClientType)
                .ToList();

            return Ok(report);
        }

        // GET: api/reports/ExportClientsReportWord
        [HttpGet("ExportClientsReportWord")]
        public async Task<IActionResult> ExportClientsReportWord(
            int? clientType = null,
            string? reservationActivity = null)
        {
            var now = DateTime.Now;

            var clientsQuery = _context.Clients.AsQueryable();

            if (clientType.HasValue && clientType.Value > 0)
            {
                clientsQuery = clientsQuery.Where(c => (int)c.Type == clientType.Value);
            }

            var clients = await clientsQuery.ToListAsync();

            var activeClientIds = await _context.PhoneNumberReservations
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .Select(r => r.ClientId)
                .Distinct()
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(reservationActivity))
            {
                reservationActivity = reservationActivity.Trim().ToLower();

                if (reservationActivity == "with-active")
                {
                    clients = clients
                        .Where(c => activeClientIds.Contains(c.Id))
                        .ToList();
                }
                else if (reservationActivity == "without-active")
                {
                    clients = clients
                        .Where(c => !activeClientIds.Contains(c.Id))
                        .ToList();
                }
            }

            var report = clients
                .GroupBy(c => c.Type)
                .Select(group => new ClientsReportDto
                {
                    ClientType = group.Key.ToString(),
                    NumberOfClients = group.Count(),
                    ClientsWithActiveReservations = group.Count(c => activeClientIds.Contains(c.Id)),
                    ClientsWithoutActiveReservations = group.Count(c => !activeClientIds.Contains(c.Id))
                })
                .OrderBy(r => r.ClientType)
                .ToList();

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

                        .footer {
                            margin-top: 25px;
                            font-size: 12px;
                            color: #777;
                        }
                    </style>
                </head>
                <body>
            ");

            html.Append("<h1>Clients Report</h1>");
            html.Append("<p class='subtitle'>Generated on " + now.ToString("yyyy-MM-dd HH:mm") + "</p>");

            html.Append("<table>");
            html.Append(@"
                <tr>
                    <th>Client Type</th>
                    <th>Number of Clients</th>
                    <th>With Active Reservations</th>
                    <th>Without Active Reservations</th>
                </tr>
            ");

            foreach (var row in report)
            {
                html.Append("<tr>");
                html.Append("<td>" + row.ClientType + "</td>");
                html.Append("<td>" + row.NumberOfClients + "</td>");
                html.Append("<td>" + row.ClientsWithActiveReservations + "</td>");
                html.Append("<td>" + row.ClientsWithoutActiveReservations + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("<p class='footer'>Total Rows: " + report.Count + "</p>");
            html.Append("</body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            var fileName = "Clients_Report_" + now.ToString("yyyyMMdd_HHmm") + ".doc";

            return File(bytes, "application/msword", fileName);
        }

        // GET: api/reports/GetDevicesReport
        [HttpGet("GetDevicesReport")]
        public async Task<IActionResult> GetDevicesReport(
            int? categoryId = null,
            int? deviceId = null,
            string? phoneStatus = null)
        {
            var now = DateTime.Now;

            var activeReservedPhoneNumberIds = await _context.PhoneNumberReservations
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .Select(r => r.PhoneNumberId)
                .Distinct()
                .ToListAsync();

            var query = _context.PhoneNumbers
                .Include(p => p.Device)
                    .ThenInclude(d => d!.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.Device != null &&
                                         p.Device.CategoryId == categoryId.Value);
            }

            if (deviceId.HasValue && deviceId.Value > 0)
            {
                query = query.Where(p => p.DeviceId == deviceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(phoneStatus))
            {
                phoneStatus = phoneStatus.Trim().ToLower();

                if (phoneStatus == "reserved")
                {
                    query = query.Where(p => activeReservedPhoneNumberIds.Contains(p.Id));
                }
                else if (phoneStatus == "unreserved")
                {
                    query = query.Where(p => !activeReservedPhoneNumberIds.Contains(p.Id));
                }
            }

            var phoneNumbers = await query.ToListAsync();

            var report = phoneNumbers
                .GroupBy(p => new
                {
                    CategoryName = p.Device != null && p.Device.Category != null
                        ? p.Device.Category.Name
                        : "No Category",

                    DeviceName = p.Device != null
                        ? p.Device.Name
                        : "No Device"
                })
                .Select(group =>
                {
                    int total = group.Count();

                    int reserved = group.Count(p =>
                        activeReservedPhoneNumberIds.Contains(p.Id));

                    int unreserved = total - reserved;

                    string availability;

                    if (reserved > 0 && unreserved > 0)
                    {
                        availability = "Partially Reserved";
                    }
                    else if (reserved > 0 && unreserved == 0)
                    {
                        availability = "Fully Reserved";
                    }
                    else
                    {
                        availability = "Available";
                    }

                    return new DevicesReportDto
                    {
                        CategoryName = group.Key.CategoryName,
                        DeviceName = group.Key.DeviceName,
                        ReservedPhoneNumbers = reserved,
                        UnreservedPhoneNumbers = unreserved,
                        TotalPhoneNumbers = total,
                        DeviceAvailability = availability
                    };
                })
                .OrderBy(r => r.CategoryName)
                .ThenBy(r => r.DeviceName)
                .ToList();

            return Ok(report);
        }

        // GET: api/reports/GetReportsOverview
        [HttpGet("GetReportsOverview")]
        public async Task<IActionResult> GetReportsOverview()
        {
            var now = DateTime.Now;

            var activeReservations = await _context.PhoneNumberReservations
                .Include(r => r.Client)
                .Include(r => r.PhoneNumber)
                    .ThenInclude(p => p!.Device)
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .ToListAsync();

            var activePhoneNumberIds = activeReservations
                .Select(r => r.PhoneNumberId)
                .Distinct()
                .ToList();

            var totalPhoneNumbers = await _context.PhoneNumbers.CountAsync();

            var reservedPhoneNumbers = activePhoneNumberIds.Count;

            var availablePhoneNumbers = totalPhoneNumbers - reservedPhoneNumbers;

            var activeClientIds = activeReservations
                .Select(r => r.ClientId)
                .Distinct()
                .ToList();

            var totalClients = await _context.Clients.CountAsync();

            var clientsWithActiveReservations = activeClientIds.Count;

            var clientsWithoutActiveReservations = totalClients - clientsWithActiveReservations;

            var mostActiveClient = activeReservations
                .GroupBy(r => new
                {
                    r.ClientId,
                    ClientName = r.Client != null ? r.Client.Name : "Unknown Client"
                })
                .Select(g => new
                {
                    ClientName = g.Key.ClientName,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            var mostReservedDevice = activeReservations
                .Where(r => r.PhoneNumber != null && r.PhoneNumber.Device != null)
                .GroupBy(r => new
                {
                    r.PhoneNumber!.DeviceId,
                    DeviceName = r.PhoneNumber.Device!.Name
                })
                .Select(g => new
                {
                    DeviceName = g.Key.DeviceName,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            var overview = new ReportsOverviewDto
            {
                AvailablePhoneNumbers = availablePhoneNumbers,

                ReservedPhoneNumbers = reservedPhoneNumbers,

                ClientsWithActiveReservations = clientsWithActiveReservations,

                ClientsWithoutActiveReservations = clientsWithoutActiveReservations,

                MostActiveClientName = mostActiveClient != null
                    ? mostActiveClient.ClientName
                    : "No active client",

                MostActiveClientReservations = mostActiveClient != null
                    ? mostActiveClient.Count
                    : 0,

                MostReservedDeviceName = mostReservedDevice != null
                    ? mostReservedDevice.DeviceName
                    : "No reserved device",

                MostReservedDeviceCount = mostReservedDevice != null
                    ? mostReservedDevice.Count
                    : 0
            };

            return Ok(overview);
        }

        // GET: api/reports/ExportDevicesReportWord
        [HttpGet("ExportDevicesReportWord")]
        public async Task<IActionResult> ExportDevicesReportWord(
            int? categoryId = null,
            int? deviceId = null,
            string? phoneStatus = null)
        {
            var now = DateTime.Now;

            var activeReservedPhoneNumberIds = await _context.PhoneNumberReservations
                .Where(r =>
                    r.BED <= now &&
                    (!r.EED.HasValue || r.EED.Value > now))
                .Select(r => r.PhoneNumberId)
                .Distinct()
                .ToListAsync();

            var query = _context.PhoneNumbers
                .Include(p => p.Device)
                    .ThenInclude(d => d!.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.Device != null &&
                                         p.Device.CategoryId == categoryId.Value);
            }

            if (deviceId.HasValue && deviceId.Value > 0)
            {
                query = query.Where(p => p.DeviceId == deviceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(phoneStatus))
            {
                phoneStatus = phoneStatus.Trim().ToLower();

                if (phoneStatus == "reserved")
                {
                    query = query.Where(p => activeReservedPhoneNumberIds.Contains(p.Id));
                }
                else if (phoneStatus == "unreserved")
                {
                    query = query.Where(p => !activeReservedPhoneNumberIds.Contains(p.Id));
                }
            }

            var phoneNumbers = await query.ToListAsync();

            var report = phoneNumbers
                .GroupBy(p => new
                {
                    CategoryName = p.Device != null && p.Device.Category != null
                        ? p.Device.Category.Name
                        : "No Category",

                    DeviceName = p.Device != null
                        ? p.Device.Name
                        : "No Device"
                })
                .Select(group =>
                {
                    int total = group.Count();

                    int reserved = group.Count(p =>
                        activeReservedPhoneNumberIds.Contains(p.Id));

                    int unreserved = total - reserved;

                    string availability;

                    if (reserved > 0 && unreserved > 0)
                    {
                        availability = "Partially Reserved";
                    }
                    else if (reserved > 0 && unreserved == 0)
                    {
                        availability = "Fully Reserved";
                    }
                    else
                    {
                        availability = "Available";
                    }

                    return new DevicesReportDto
                    {
                        CategoryName = group.Key.CategoryName,
                        DeviceName = group.Key.DeviceName,
                        ReservedPhoneNumbers = reserved,
                        UnreservedPhoneNumbers = unreserved,
                        TotalPhoneNumbers = total,
                        DeviceAvailability = availability
                    };
                })
                .OrderBy(r => r.CategoryName)
                .ThenBy(r => r.DeviceName)
                .ToList();

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

                .footer {
                    margin-top: 25px;
                    font-size: 12px;
                    color: #777;
                }
            </style>
        </head>
        <body>
    ");

            html.Append("<h1>Devices & Phone Numbers Report</h1>");
            html.Append("<p class='subtitle'>Generated on " + now.ToString("yyyy-MM-dd HH:mm") + "</p>");

            html.Append("<table>");
            html.Append(@"
        <tr>
            <th>Category</th>
            <th>Device</th>
            <th>Reserved Phone Numbers</th>
            <th>Unreserved Phone Numbers</th>
            <th>Total Phone Numbers</th>
            <th>Device Availability</th>
        </tr>
    ");

            foreach (var row in report)
            {
                html.Append("<tr>");
                html.Append("<td>" + row.CategoryName + "</td>");
                html.Append("<td>" + row.DeviceName + "</td>");
                html.Append("<td>" + row.ReservedPhoneNumbers + "</td>");
                html.Append("<td>" + row.UnreservedPhoneNumbers + "</td>");
                html.Append("<td>" + row.TotalPhoneNumbers + "</td>");
                html.Append("<td>" + row.DeviceAvailability  + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("<p class='footer'>Total Rows: " + report.Count + "</p>");
            html.Append("</body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            var fileName = "Devices_Report_" + now.ToString("yyyyMMdd_HHmm") + ".doc";

            return File(bytes, "application/msword", fileName);
        }
    }
}