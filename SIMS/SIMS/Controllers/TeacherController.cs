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
        private const string UploadPath = "uploads/teachers"; // Hằng số cho đường dẫn upload
        private readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" }; // Định dạng file cho phép
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public TeacherController(SimDatacontext context, ILogger<TeacherController> logger, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Teacher - Hiển thị danh sách giáo viên
        public IActionResult Index()
        {
            try
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

                _logger.LogInformation("Loaded {Count} teachers from database", teacherList.Count);
                var model = new TeacherViewModel { TeacherList = teacherList };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading teacher list");
                TempData["error"] = "Error loading teachers. Please try again.";
                return View(new TeacherViewModel { TeacherList = new List<TeacherDetail>() });
            }
        }

        // GET: Teacher/Add - Hiển thị form thêm giáo viên
        [HttpGet]
        public IActionResult Add()
        {
            return View(new TeacherDetail());
        }

        // POST: Teacher/Add - Xử lý thêm giáo viên mới
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

                // Kiểm tra file avatar
                if (ViewAvatar == null || ViewAvatar.Length == 0)
                {
                    ModelState.AddModelError("ViewAvatar", "Please choose an avatar.");
                }
                else
                {
                    // Kiểm tra định dạng file
                    var fileExtension = Path.GetExtension(ViewAvatar.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ViewAvatar", "Only .jpg, .jpeg, .png files are allowed.");
                    }
                    // Kiểm tra kích thước file
                    if (ViewAvatar.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("ViewAvatar", "Avatar file size must be less than 5MB.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state when adding teacher");
                    return View(model);
                }

                try
                {
                    string? avatarFileName = null;

                    // Xử lý upload file avatar
                    if (ViewAvatar != null && ViewAvatar.Length > 0)
                    {
                        var uploadDir = Path.Combine(_hostEnvironment.WebRootPath, UploadPath);
                        Directory.CreateDirectory(uploadDir);

                        avatarFileName = $"{Guid.NewGuid()}{Path.GetExtension(ViewAvatar.FileName)}";
                        var filePath = Path.Combine(uploadDir, avatarFileName);

                        _logger.LogInformation("Attempting to save avatar to {Path}", filePath);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ViewAvatar.CopyToAsync(stream);
                        }
                        _logger.LogInformation("Uploaded avatar: {FileName} to {Path}", avatarFileName, filePath);

                        // Kiểm tra file có thực sự được lưu không
                        if (!System.IO.File.Exists(filePath))
                        {
                            _logger.LogError("Avatar file was not saved to {Path}", filePath);
                            throw new Exception("Failed to save avatar file.");
                        }
                    }

                    // Tạo entity Teacher
                    var teacher = new Teacher
                    {
                        TeacherName = model.Name,
                        Email = model.Email,
                        Address = model.Address,
                        Subject = model.Subject,
                        Avatar = avatarFileName,
                        CreatedAt = DateTime.Now
                    };

                    // Lưu vào database
                    await _context.Teacher.AddAsync(teacher);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Added teacher: {Id}, {Name}, {Email}", teacher.Id, teacher.TeacherName, teacher.Email);

                    TempData["success"] = "Teacher added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding teacher");
                    TempData["error"] = $"Error adding teacher: {ex.Message}";
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Teacher/Edit/5 - Hiển thị form chỉnh sửa giáo viên
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var teacher = _context.Teacher
                .FirstOrDefault(t => t.Id == id && t.DeletedAt == null);

            if (teacher == null)
            {
                _logger.LogWarning("Teacher not found: {Id}", id);
                return NotFound();
            }

            var model = new TeacherDetail
            {
                Id = teacher.Id,
                Name = teacher.TeacherName,
                Email = teacher.Email,
                Address = teacher.Address,
                Subject = teacher.Subject,
                Avatar = teacher.Avatar
            };

            return View(model);
        }

        // POST: Teacher/Edit/5 - Xử lý chỉnh sửa giáo viên
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

                // Kiểm tra file avatar nếu có
                if (ViewAvatar != null && ViewAvatar.Length > 0)
                {
                    var fileExtension = Path.GetExtension(ViewAvatar.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ViewAvatar", "Only .jpg, .jpeg, .png files are allowed.");
                    }
                    if (ViewAvatar.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("ViewAvatar", "Avatar file size must be less than 5MB.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state when editing teacher: {Id}", model.Id);
                    return View(model);
                }

                try
                {
                    var teacher = await _context.Teacher.FirstOrDefaultAsync(t => t.Id == model.Id && t.DeletedAt == null);
                    if (teacher == null)
                    {
                        _logger.LogWarning("Teacher not found for edit: {Id}", model.Id);
                        return NotFound();
                    }

                    // Cập nhật thông tin cơ bản
                    teacher.TeacherName = model.Name;
                    teacher.Email = model.Email;
                    teacher.Address = model.Address;
                    teacher.Subject = model.Subject;
                    teacher.UpdatedAt = DateTime.Now;

                    // Xử lý avatar mới
                    _logger.LogInformation("ViewAvatar is {State}", ViewAvatar == null ? "null" : "not null");
                    if (ViewAvatar != null && ViewAvatar.Length > 0)
                    {
                        var uploadDir = Path.Combine(_hostEnvironment.WebRootPath, UploadPath);
                        Directory.CreateDirectory(uploadDir);

                        // Xóa avatar cũ
                        if (!string.IsNullOrEmpty(teacher.Avatar))
                        {
                            var oldFilePath = Path.Combine(uploadDir, teacher.Avatar);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                                _logger.LogInformation("Deleted old avatar: {FileName}", teacher.Avatar);
                            }
                            else
                            {
                                _logger.LogWarning("Old avatar file not found: {Path}", oldFilePath);
                            }
                        }

                        // Lưu avatar mới
                        var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(ViewAvatar.FileName)}";
                        var newFilePath = Path.Combine(uploadDir, newFileName);

                        _logger.LogInformation("Attempting to save new avatar to {Path}", newFilePath);
                        using (var stream = new FileStream(newFilePath, FileMode.Create))
                        {
                            await ViewAvatar.CopyToAsync(stream);
                        }
                        _logger.LogInformation("Uploaded new avatar: {FileName} to {Path}", newFileName, newFilePath);

                        // Kiểm tra file có thực sự được lưu không
                        if (!System.IO.File.Exists(newFilePath))
                        {
                            _logger.LogError("New avatar file was not saved to {Path}", newFilePath);
                            throw new Exception("Failed to save new avatar file.");
                        }

                        teacher.Avatar = newFileName;
                    }
                    else
                    {
                        _logger.LogInformation("No new avatar uploaded, keeping existing avatar: {Avatar}", teacher.Avatar);
                    }

                    // Cập nhật database
                    _context.Teacher.Update(teacher);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated teacher: {Id}, {Name}, {Email}", teacher.Id, teacher.TeacherName, teacher.Email);

                    TempData["success"] = "Teacher updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating teacher: {Id}", model.Id);
                    TempData["error"] = $"Error updating teacher: {ex.Message}";
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            return View(model);
        }

        // POST: Teacher/Delete/5 - Xử lý xóa giáo viên (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var teacher = await _context.Teacher.FindAsync(id);
                if (teacher == null || teacher.DeletedAt != null)
                {
                    _logger.LogWarning("Teacher not found for deletion: {Id}", id);
                    return NotFound();
                }

                teacher.DeletedAt = DateTime.Now;
                _context.Teacher.Update(teacher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted teacher: {Id}, {Name}", teacher.Id, teacher.TeacherName);

                TempData["success"] = "Teacher deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting teacher: {Id}", id);
                TempData["error"] = $"Error deleting teacher: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}