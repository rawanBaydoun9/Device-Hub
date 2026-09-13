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
    [Route("api/cart")]
    public class CartApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/cart/GetMyCart
        [HttpGet("GetMyCart")]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Device)
                    .ThenInclude(d => d!.Category)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AddedAt)
                .Select(c => new CartItemDto
                {
                    Id = c.Id,
                    DeviceId = c.DeviceId,
                    DeviceName = c.Device != null ? c.Device.Name : "",
                    CategoryName = c.Device != null && c.Device.Category != null ? c.Device.Category.Name : "",
                    Quantity = c.Quantity,
                    AvailableQuantity = c.Device != null ? c.Device.Quantity : 0,
                    AddedAt = c.AddedAt
                })
                .ToListAsync();

            return Ok(cartItems);
        }

        // POST: api/cart/AddToCart/5
        [HttpPost("AddToCart/{deviceId}")]
        public async Task<IActionResult> AddToCart(int deviceId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            var device = await _context.Devices.FindAsync(deviceId);

            if (device == null)
            {
                return NotFound("Device not found.");
            }

            if (!device.ShowOnUserHome)
            {
                return BadRequest("This device is not available for users.");
            }

            if (device.Quantity <= 0)
            {
                return BadRequest("This device is out of stock.");
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.DeviceId == deviceId);

            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity >= device.Quantity)
                {
                    return BadRequest("You cannot add more than the available quantity.");
                }

                existingCartItem.Quantity++;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    DeviceId = deviceId,
                    Quantity = 1,
                    AddedAt = DateTime.Now
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{device.Name} added to cart."
            });
        }

        // POST: api/cart/SubmitRequest
        [HttpPost("SubmitRequest")]
        public async Task<IActionResult> SubmitRequest()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            var cartItems = await _context.CartItems
                .Include(c => c.Device)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest("Your cart is empty.");
            }

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Device == null)
                {
                    return BadRequest("One of the selected devices was not found.");
                }

                if (!cartItem.Device.ShowOnUserHome)
                {
                    return BadRequest($"{cartItem.Device.Name} is no longer available.");
                }

                if (cartItem.Quantity > cartItem.Device.Quantity)
                {
                    return BadRequest($"{cartItem.Device.Name} does not have enough available quantity.");
                }
            }

            var request = new DeviceRequest
            {
                UserId = userId,
                Status = "Pending",
                RequestedAt = DateTime.Now
            };

            foreach (var cartItem in cartItems)
            {
                request.Items.Add(new DeviceRequestItem
                {
                    DeviceId = cartItem.DeviceId,
                    Quantity = cartItem.Quantity
                });
            }

            _context.DeviceRequests.Add(request);

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Your request has been submitted successfully."
            });
        }

        // PUT: api/cart/UpdateQuantity/5?quantity=2
        [HttpPut("UpdateQuantity/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            if (quantity < 1)
            {
                return BadRequest("Quantity must be at least 1.");
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Device)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            if (cartItem.Device == null)
            {
                return BadRequest("Device not found.");
            }

            if (quantity > cartItem.Device.Quantity)
            {
                return BadRequest("Quantity cannot exceed available stock.");
            }

            cartItem.Quantity = quantity;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cart quantity updated."
            });
        }

        // DELETE: api/cart/RemoveFromCart/5
        [HttpDelete("RemoveFromCart/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Item removed from cart."
            });
        }

        // DELETE: api/cart/ClearCart
        [HttpDelete("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User is not logged in.");
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cart cleared."
            });
        }
    }
}