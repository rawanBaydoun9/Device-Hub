using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task2.Controllers
{
    [Authorize]
    public class DevicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}