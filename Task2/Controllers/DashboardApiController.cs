using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2.Data;
using Task2.DTOs;

namespace Task2.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/GetSummary
        [HttpGet("GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.Now;

            var summary = new DashboardSummaryDto
            {
                TotalDevices = await _context.Devices.CountAsync(),

                TotalCategories = await _context.Categories.CountAsync(),

                TotalClients = await _context.Clients.CountAsync(),

                TotalPhoneNumbers = await _context.PhoneNumbers.CountAsync(),

                ActiveReservations = await _context.PhoneNumberReservations
                    .CountAsync(r =>
                        r.BED <= now &&
                        (!r.EED.HasValue || r.EED.Value > now)),

                EndedReservations = await _context.PhoneNumberReservations
                    .CountAsync(r =>
                        r.EED.HasValue &&
                        r.EED.Value <= now)
            };

            return Ok(summary);
        }
    }
}