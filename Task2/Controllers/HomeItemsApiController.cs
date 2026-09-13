using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2.Data;
using Task2.DTOs;
using Task2.Models;

namespace Task2.Controllers
{
    [Authorize]
    [Route("api/homeItems")]
    [ApiController]
    public class HomeItemsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeItemsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetHomeItems")]
        public async Task<IActionResult> GetHomeItems()
        {
            await SeedDefaultItemsIfNeeded();

            var isAdmin = User.IsInRole("Admin");

            var query = _context.HomeItemSettings.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(h => h.IsVisible);
            }

            var items = await query
                .OrderBy(h => h.DisplayOrder)
                .Select(h => new HomeItemSettingDto
                {
                    Id = h.Id,
                    Key = h.Key,
                    Title = h.Title,
                    Description = h.Description,
                    IconClass = h.IconClass,
                    ControllerName = h.ControllerName,
                    ActionName = h.ActionName,
                    IsVisible = h.IsVisible,
                    DisplayOrder = h.DisplayOrder
                })
                .ToListAsync();

            return Ok(new
            {
                isAdmin,
                items
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ToggleVisibility/{id}")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var item = await _context.HomeItemSettings.FindAsync(id);

            if (item == null)
            {
                return NotFound(new { message = "Home item not found." });
            }

            item.IsVisible = !item.IsVisible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = item.IsVisible
                    ? $"{item.Title} is now visible."
                    : $"{item.Title} is now hidden.",
                item.IsVisible
            });
        }

        private async Task SeedDefaultItemsIfNeeded()
        {
            if (await _context.HomeItemSettings.AnyAsync())
            {
                return;
            }

            var items = new List<HomeItemSetting>
            {
                new HomeItemSetting
                {
                    Key = "devices",
                    Title = "Devices",
                    Description = "Manage device inventory and quantities.",
                    IconClass = "fa-solid fa-laptop",
                    ControllerName = "Devices",
                    ActionName = "Index",
                    IsVisible = true,
                    DisplayOrder = 1
                },
                new HomeItemSetting
                {
                    Key = "categories",
                    Title = "Categories",
                    Description = "Organize devices by category.",
                    IconClass = "fa-solid fa-layer-group",
                    ControllerName = "Categories",
                    ActionName = "Index",
                    IsVisible = true,
                    DisplayOrder = 2
                },
                new HomeItemSetting
                {
                    Key = "clients",
                    Title = "Clients",
                    Description = "Manage individual and organization clients.",
                    IconClass = "fa-solid fa-users",
                    ControllerName = "Clients",
                    ActionName = "Index",
                    IsVisible = true,
                    DisplayOrder = 3
                },
                new HomeItemSetting
                {
                    Key = "phoneNumbers",
                    Title = "Phone Numbers",
                    Description = "Manage phone numbers linked to devices.",
                    IconClass = "fa-solid fa-phone",
                    ControllerName = "PhoneNumbers",
                    ActionName = "Index",
                    IsVisible = true,
                    DisplayOrder = 4
                },
                new HomeItemSetting
                {
                    Key = "reports",
                    Title = "Reports",
                    Description = "View reports, exports, and import tools.",
                    IconClass = "fa-solid fa-chart-column",
                    ControllerName = "Reports",
                    ActionName = "Index",
                    IsVisible = true,
                    DisplayOrder = 5
                }
            };

            _context.HomeItemSettings.AddRange(items);
            await _context.SaveChangesAsync();
        }
    }
}