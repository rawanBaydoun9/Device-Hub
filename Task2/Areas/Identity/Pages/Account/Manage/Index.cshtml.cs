using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Task2.Models;

namespace Task2.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Mobile Number")]
            public string PhoneNumber { get; set; } = string.Empty;
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName ?? string.Empty;
            IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            Input = new InputModel
            {
                FullName = IsAdmin ? "System Admin" : user.FullName,
                PhoneNumber = IsAdmin ? "" : phoneNumber ?? string.Empty
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (IsAdmin)
            {
                user.FullName = "System Admin";
                user.PhoneNumber = null;

                var adminUpdateResult = await _userManager.UpdateAsync(user);

                if (!adminUpdateResult.Succeeded)
                {
                    StatusMessage = "Error: Failed to update admin profile.";
                    return RedirectToPage();
                }

                await _signInManager.RefreshSignInAsync(user);

                StatusMessage = "System admin profile is already updated.";

                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Input.FullName))
            {
                ModelState.AddModelError("Input.FullName", "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Mobile number is required.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.FullName = Input.FullName.Trim();

            var updateProfileResult = await _userManager.UpdateAsync(user);

            if (!updateProfileResult.Succeeded)
            {
                StatusMessage = "Error: Failed to update profile information.";
                return RedirectToPage();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (Input.PhoneNumber.Trim() != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber.Trim());

                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Error: Failed to update mobile number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your profile has been updated.";

            return RedirectToPage();
        }
    }
}