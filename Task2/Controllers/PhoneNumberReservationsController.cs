using Microsoft.AspNetCore.Mvc;

namespace Task2.Controllers
{
    public class PhoneNumberReservationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}