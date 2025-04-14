using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SIMS.Database;
using SIMS.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class TeacherController : Controller
    {
        private readonly SimDatacontext _context;
        private readonly ILogger<TeacherController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public TeacherController(SimDatacontext context, ILogger<TeacherController> logger, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Teacher
        public IActionResult Index()
        {
            var teacherList = _context.Teacher
                .Where(t => t.DeletedAt == null)
                .Select(t => new TeacherDetail
                {
                    Id = t.Id,
                    Name = t.TeacherName,
                    Email = t.Email,
                    Address = t.Address,
                    Subject = t.Subject,
                    Avatar = t.Avatar
                }).ToList();

            var model = new TeacherViewModel { TeacherList = teacherList };
            return View(model);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TeacherDetail model, IFormFile? ViewAvatar)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                bool emailExists = _context.Teacher.Any(t =>
                    t.Email == model.Email &&
                    t.DeletedAt == null);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists!");
                }

                // Kiểm tra ảnh bắt buộc
                if (model.ViewAvatar == null || model.ViewAvatar.Length == 0)
                {
                    ModelState.AddModelError("ViewAvatar", "Please choose an avatar.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    string? avatarFileName = null;

                    if (ViewAvatar != null && ViewAvatar.Length > 0)
                    {
                        var uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "teachers");
                        Directory.CreateDirectory(uploadDir);

                        avatarFileName = $"{Guid.NewGuid()}{Path.GetExtension(ViewAvatar.FileName)}";
                        var filePath = Path.Combine(uploadDir, avatarFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ViewAvatar.CopyToAsync(stream);
                        }
                    }

                    var teacher = new Teacher
                    {
                        TeacherName = model.Name,
                        Email = model.Email,
                        Address = model.Address,
                        Subject = model.Subject,
                        Avatar = avatarFileName,
                        CreatedAt = DateTime.Now
                    };

                    await _context.Teacher.AddAsync(teacher);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Teacher added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Error adding teacher: {ex.Message}";
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Teacher/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var teacher = _context.Teacher
                .FirstOrDefault(t => t.Id == id && t.DeletedAt == null);

            if (teacher == null)
            {
                return NotFound();
            }

            var model = new TeacherDetail
            {
                Id = teacher.Id,
                Name = teacher.TeacherName,
                Email = teacher.Email,
                Address = teacher.Address,
                Subject = teacher.Subject,
                Avatar = teacher.Avatar,
            };

            return View(model);
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeacherDetail model, IFormFile? ViewAvatar)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                bool emailExists = _context.Teacher.Any(t =>
                    t.Email == model.Email &&
                    t.Id != model.Id &&
                    t.DeletedAt == null);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists!");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    var teacher = await _context.Teacher.FirstOrDefaultAsync(t => t.Id == model.Id && t.DeletedAt == null);
                    if (teacher == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin cơ bản
                    teacher.TeacherName = model.Name;
                    teacher.Email = model.Email;
                    teacher.Address = model.Address;
                    teacher.Subject = model.Subject;
                    teacher.UpdatedAt = DateTime.Now;

                    // Nếu có ảnh mới
                    if (ViewAvatar != null && ViewAvatar.Length > 0)
                    {
                        var uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "teachers");
                        Directory.CreateDirectory(uploadDir);

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(teacher.Avatar))
                        {
                            var oldFilePath = Path.Combine(uploadDir, teacher.Avatar);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(ViewAvatar.FileName)}";
                        var newFilePath = Path.Combine(uploadDir, newFileName);

                        using (var stream = new FileStream(newFilePath, FileMode.Create))
                        {
                            await ViewAvatar.CopyToAsync(stream);
                        }

                        teacher.Avatar = newFileName;
                    }

                    _context.Teacher.Update(teacher);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Teacher updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Error updating teacher: {ex.Message}";
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            return View(model);
        }


        // POST: Teacher/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.DeletedAt = DateTime.Now;

            _context.Teacher.Update(teacher);
            await _context.SaveChangesAsync();

            TempData["success"] = "Teacher deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
