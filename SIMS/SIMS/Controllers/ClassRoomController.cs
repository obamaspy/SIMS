using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIMS.Controllers
{
    public class ClassRoomController : Controller
    {
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
