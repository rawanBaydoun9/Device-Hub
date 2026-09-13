using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2.DTOs;
using Task2.Models;

namespace Task2.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersApiController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/users/GetAllUsers
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var result = new List<UserManagementDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserManagementDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role = roles.FirstOrDefault() ?? "User",
                    CreatedAt = user.CreatedAt
                });
            }

            return Ok(result);
        }

        // POST: api/users/CreateUser
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please fill all required fields correctly.");
            }

            dto.FullName = dto.FullName.Trim();
            dto.Email = dto.Email.Trim();
            dto.PhoneNumber = dto.PhoneNumber.Trim();

            var emailExists = await _userManager.FindByEmailAsync(dto.Email);

            if (emailExists != null)
            {
                return BadRequest("Email already exists.");
            }

            var userRoleExists = await _roleManager.RoleExistsAsync("User");

            if (!userRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(errorMessage);
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new
            {
                message = "User created successfully."
            });
        }
    }
}