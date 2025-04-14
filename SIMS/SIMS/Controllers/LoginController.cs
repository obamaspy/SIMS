using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Database;
using SIMS.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SIMS.Controllers
{
    public class LoginController : Controller
    {

        private readonly SimDatacontext _context;

        public LoginController(SimDatacontext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu người dùng nhập vào
                var hashedPassword = HashPassword(model.Password);

                // Tìm tài khoản khớp email và mật khẩu băm
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Email == model.Email && a.Password == hashedPassword);

                if (account != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.Email),
                        new Claim(ClaimTypes.Role, account.Role)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    if (account.Role == "Admin" || account.Role == "Teacher")
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (account.Role == "Student")
                    {
                        return RedirectToAction("ViewProfile", "Student");
                    }
                }
                else
                {
                    ViewData["MessageLogin"] = "Account or password is invalid.";
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
