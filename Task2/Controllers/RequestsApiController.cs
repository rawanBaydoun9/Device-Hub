using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2.Data;
using Task2.DTOs;

namespace Task2.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/requests")]
    public class RequestsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/requests/GetAllRequests
        [HttpGet("GetAllRequests")]
        public async Task<IActionResult> GetAllRequests(string? status = "")
        {
            var query = _context.DeviceRequests
                .Include(r => r.User)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Device)
                        .ThenInclude(d => d!.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new DeviceRequestDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Email ?? "" : "",
                    FullName = r.User != null ? r.User.FullName : "",
                    PhoneNumber = r.User != null ? r.User.PhoneNumber ?? "" : "",
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    Items = r.Items.Select(i => new DeviceRequestItemDto
                    {
                        Id = i.Id,
                        DeviceId = i.DeviceId,
                        DeviceName = i.Device != null ? i.Device.Name : "",
                        CategoryName = i.Device != null && i.Device.Category != null ? i.Device.Category.Name : "",
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(requests);
        }

        // POST: api/requests/AcceptRequest/5
        [HttpPost("AcceptRequest/{id}")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.DeviceRequests
                .Include(r => r.Items)
                    .ThenInclude(i => i.Device)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound("Request not found.");
            }

            if (request.Status != "Pending")
            {
                return BadRequest("Only pending requests can be accepted.");
            }

            foreach (var item in request.Items)
            {
                if (item.Device == null)
                {
                    return BadRequest("One of the requested devices was not found.");
                }

                if (item.Quantity > item.Device.Quantity)
                {
                    return BadRequest($"{item.Device.Name} does not have enough available quantity.");
                }
            }

            foreach (var item in request.Items)
            {
                item.Device!.Quantity -= item.Quantity;
            }

            request.Status = "Accepted";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request accepted successfully."
            });
        }

        // POST: api/requests/RejectRequest/5
        [HttpPost("RejectRequest/{id}")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.DeviceRequests.FindAsync(id);

            if (request == null)
            {
                return NotFound("Request not found.");
            }

            if (request.Status != "Pending")
            {
                return BadRequest("Only pending requests can be rejected.");
            }

            request.Status = "Rejected";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request rejected successfully."
            });
        }
    }
}