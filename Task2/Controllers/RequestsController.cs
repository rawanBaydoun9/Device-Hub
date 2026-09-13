using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}