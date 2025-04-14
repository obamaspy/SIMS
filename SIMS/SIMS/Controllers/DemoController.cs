using Microsoft.AspNetCore.Mvc;

namespace SIMS.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Index()
        {
            //return View();
            return Ok("Demo Appication");
            //domain/Demo/Index
        }
        public IActionResult Test(int ID, string Name, int Age)
        {
            return Ok("Hello SE06301 - ID" + ID + "- Name: " + Name + "- Age: " + Age);
            //Demo/Test?ID=10&Name=abc&Age=22
        }
    }
}
