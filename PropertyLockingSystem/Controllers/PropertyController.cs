using Microsoft.AspNetCore.Mvc;

namespace PropertyLockingSystem.Controllers
{
    public class PropertyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
