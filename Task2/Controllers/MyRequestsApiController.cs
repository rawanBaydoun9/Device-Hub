using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2.Data;
using Task2.DTOs;
using Task2.Models;

namespace Task2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/myRequests")]
    public class MyRequestsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyRequestsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/myRequests/GetMyRequests
        [HttpGet("GetMyRequests")]
        public async Task<IActionResult> GetMyRequests(string? status = "")
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            var query = _context.DeviceRequests
                .Include(r => r.Items)
                    .ThenInclude(i => i.Device)
                        .ThenInclude(d => d.Category)
                .Where(r => r.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            var result = requests.Select(r => new DeviceRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = User.Identity?.Name ?? "",
                FullName = "",
                PhoneNumber = "",
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                Items = r.Items.Select(i => new DeviceRequestItemDto
                {
                    Id = i.Id,
                    DeviceId = i.DeviceId,
                    DeviceName = i.Device != null ? i.Device.Name : "",
                    CategoryName = i.Device != null && i.Device.Category != null
                        ? i.Device.Category.Name
                        : "",
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}