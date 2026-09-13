using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Task2.Controllers
{

    [Authorize]
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}