using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS.Database;
using SIMS.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AccountsController : Controller
    {

        private readonly SimDatacontext _dbContext;

        public AccountsController(SimDatacontext context)
        {
            _dbContext = context;
        }

        public IActionResult Index()
        {
            var accountsModel = new AccountsViewModel
            {
                AccountsList = _dbContext.Accounts.Select(p => new AccountsDetail
                {
                    Id = p.Id,
                    Username = p.Username,
                    Email = p.Email,
                    Phone = p.Phone,
                    Password = "••••••••", // Hiển thị dấu chấm cho mật khẩu
                    Role = p.Role,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList()
            };

            ViewData["title"] = "Accounts";
            return View(accountsModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(AccountsDetail model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Phone) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.Role))
            {
                ModelState.AddModelError("", "Please enter complete information.");
                return View(model);
            }

            if (!IsStrongPassword(model.Password))
            {
                ModelState.AddModelError("Password", "Password must contain special characters, uppercase letters, lowercase letters, numbers, and be at least 8 characters long.");
                return View(model);
            }

            bool isDuplicate = _dbContext.Accounts.Any(a =>
                a.Username == model.Username ||
                a.Email == model.Email ||
                a.Phone == model.Phone);

            if (isDuplicate)
            {
                if (_dbContext.Accounts.Any(a => a.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username is already in use.");
                }
                if (_dbContext.Accounts.Any(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                }
                if (_dbContext.Accounts.Any(a => a.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone number is already in use.");
                }
                return View(model);
            }

            try
            {
                var newAccount = new Accounts
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = HashPassword(model.Password),
                    Role = model.Role,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Accounts.Add(newAccount);
                _dbContext.SaveChanges();
                TempData["success"] = "Account added successfully!";

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while adding the account.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var account = _dbContext.Accounts.Find(id);
            if (account == null)
            {
                return NotFound();
            }

            var model = new AccountsDetail
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                Phone = account.Phone,
                Password = "", // Không hiển thị mật khẩu băm
                Role = account.Role
            };

            ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" }, model.Role);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AccountsDetail model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Phone) ||
                string.IsNullOrWhiteSpace(model.Role))
            {
                ModelState.AddModelError("", "Please enter complete information.");
                ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" }, model.Role);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password) && !IsStrongPassword(model.Password))
            {
                ModelState.AddModelError("Password", "Password must contain special characters, uppercase letters, lowercase letters, numbers, and be at least 8 characters long.");
                ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" }, model.Role);
                return View(model);
            }

            var account = _dbContext.Accounts.Find(model.Id);
            if (account == null)
            {
                return NotFound();
            }

            var duplicateAccount = _dbContext.Accounts.FirstOrDefault(a =>
                (a.Username == model.Username || a.Email == model.Email || a.Phone == model.Phone) &&
                a.Id != model.Id);

            if (duplicateAccount != null)
            {
                if (duplicateAccount.Username == model.Username)
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                }
                if (duplicateAccount.Email == model.Email)
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                }
                if (duplicateAccount.Phone == model.Phone)
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered.");
                }

                ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" }, model.Role);
                return View(model);
            }

            try
            {
                account.Username = model.Username;
                account.Email = model.Email;
                account.Phone = model.Phone;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    account.Password = HashPassword(model.Password);
                }

                account.Role = model.Role;
                account.UpdatedAt = DateTime.Now;

                _dbContext.Accounts.Update(account);
                _dbContext.SaveChanges();
                TempData["success"] = "Account updated successfully!";

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the account.");
                ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" }, model.Role);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var account = _dbContext.Accounts.Find(id);
            if (account == null)
            {
                return NotFound();
            }

            try
            {
                _dbContext.Accounts.Remove(account);
                _dbContext.SaveChanges();
                TempData["success"] = "Account deleted successfully!";
            }
            catch
            {
                TempData["error"] = "An error occurred while deleting the account.";
            }

            return RedirectToAction("Index");
        }

        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, "[A-Z]") &&
                   Regex.IsMatch(password, "[a-z]") &&
                   Regex.IsMatch(password, "[0-9]") &&
                   Regex.IsMatch(password, "[^a-zA-Z0-9]");
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
