using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS.Database;
using SIMS.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class StudentController : Controller
    {
        private readonly SimDatacontext _dbContext;
        private readonly IWebHostEnvironment _env;

        public StudentController(SimDatacontext context, IWebHostEnvironment env)
        {
            _dbContext = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var studentModel = new StudentViewModel
            {
                StudentList = _dbContext.Student.Select(s => new StudentDetail
                {
                    Id = s.Id,
                    Name = s.StudentName,
                    StudentCode = s.StudentCode,
                    Email = s.Email,
                    Address = s.Address,
                    Course = s.Course,
                    Avatar = s.Avatar,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList()
            };

            ViewData["title"] = "Student";
            return View(studentModel);
        }

        private List<SelectListItem> GetCourseList()
        {
            return _dbContext.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CoursesName,
                    Text = c.CoursesName
                }).ToList();
        }

        [HttpGet]
        public IActionResult Add()
        {
            var model = new StudentDetail
            {
                CourseList = GetCourseList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(StudentDetail model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.StudentCode) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Course))
            {
                ModelState.AddModelError("", "Please enter complete information.");
            }

            if (_dbContext.Student.Any(s => s.StudentCode == model.StudentCode))
            {
                ModelState.AddModelError("StudentCode", "This student code is already taken.");
            }
            if (_dbContext.Student.Any(s => s.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }

            if (!ModelState.IsValid)
            {
                model.CourseList = GetCourseList();
                return View(model);
            }

            string avatarFileName = null;

            if (model.ViewAvatar != null)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "students");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                avatarFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ViewAvatar.FileName);
                string filePath = Path.Combine(uploadFolder, avatarFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ViewAvatar.CopyTo(fileStream);
                }
            }

            try
            {
                var newStudent = new Student
                {
                    StudentName = model.Name,
                    StudentCode = model.StudentCode,
                    Email = model.Email,
                    Address = model.Address,
                    Course = model.Course,
                    Avatar = avatarFileName, // Lưu tên file ảnh thay vì đường dẫn
                    CreatedAt = DateTime.Now
                };

                _dbContext.Student.Add(newStudent);
                _dbContext.SaveChanges();
                TempData["success"] = "Student added successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while adding the student.");
                model.CourseList = GetCourseList();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _dbContext.Student.Find(id);
            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentDetail
            {
                Id = student.Id,
                Name = student.StudentName,
                StudentCode = student.StudentCode,
                Email = student.Email,
                Address = student.Address,
                Course = student.Course,
                Avatar = student.Avatar,
                CourseList = GetCourseList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(StudentDetail model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.StudentCode) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Course))
            {
                ModelState.AddModelError("", "Please enter complete information.");
            }

            var student = _dbContext.Student.Find(model.Id);
            if (student == null)
            {
                return NotFound();
            }

            if (_dbContext.Student.Any(s => s.StudentCode == model.StudentCode && s.Id != model.Id))
            {
                ModelState.AddModelError("StudentCode", "This student code is already taken.");
            }
            if (_dbContext.Student.Any(s => s.Email == model.Email && s.Id != model.Id))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
            }

            if (!ModelState.IsValid)
            {
                model.CourseList = GetCourseList();
                model.Avatar = student.Avatar; // Giữ avatar cũ khi có lỗi
                return View(model);
            }

            if (model.ViewAvatar != null)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "students");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ViewAvatar.FileName);
                string filePath = Path.Combine(uploadFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ViewAvatar.CopyTo(fileStream);
                }

                student.Avatar = fileName; // Chỉ lưu tên file ảnh
            }

            try
            {
                student.StudentName = model.Name;
                student.StudentCode = model.StudentCode;
                student.Email = model.Email;
                student.Address = model.Address;
                student.Course = model.Course;
                student.UpdatedAt = DateTime.Now;

                _dbContext.SaveChanges();
                TempData["success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the student.");
                model.CourseList = GetCourseList();
                model.Avatar = student.Avatar;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var student = _dbContext.Student.Find(id);
            if (student == null)
            {
                return NotFound();
            }

            try
            {
                _dbContext.Student.Remove(student);
                _dbContext.SaveChanges();
                TempData["success"] = "Student deleted successfully!";
            }
            catch
            {
                TempData["error"] = "An error occurred while deleting the student.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ViewProfile()
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login");
            }

            var student = _dbContext.Student.FirstOrDefault(s => s.Email == email);

            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentDetail
            {
                Id = student.Id,
                Name = student.StudentName,
                StudentCode = student.StudentCode,
                Email = student.Email,
                Address = student.Address,
                Course = student.Course,
                Avatar = student.Avatar
            };

            ViewData["title"] = "Student Profile";
            return View("ViewProfile", model);
        }
    }
}
