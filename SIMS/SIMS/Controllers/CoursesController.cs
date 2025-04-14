using Microsoft.AspNetCore.Mvc;
using SIMS.Database;
using SIMS.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class CoursesController : Controller
    {
        private readonly SimDatacontext _dbContext;

        public CoursesController(SimDatacontext context)
        {
            _dbContext = context;
        }

        public IActionResult Index()
        {
            var coursesModel = new CoursesViewModel
            {
                CoursesList = _dbContext.Courses.Select(c => new CoursesDetail
                {
                    Id = c.Id,
                    CoursesName = c.CoursesName,
                    Subject = c.Subject,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate
                }).ToList()
            };

            ViewData["title"] = "Courses";
            return View(coursesModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(CoursesDetail model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please enter valid data.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.CoursesName) ||
                string.IsNullOrWhiteSpace(model.Subject) ||
                model.StartDate == default || model.EndDate == default)
            {
                TempData["error"] = "All fields are required.";
                return View(model);
            }

            if (model.StartDate >= model.EndDate)
            {
                TempData["error"] = "Start date must be before the end date.";
                return View(model);
            }

            if (_dbContext.Courses.Any(c => c.CoursesName == model.CoursesName))
            {
                TempData["error"] = "Course name already exists. Please choose a different name.";
                return View(model);
            }

            try
            {
                var newCourse = new Courses
                {
                    CoursesName = model.CoursesName,
                    Subject = model.Subject,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Courses.Add(newCourse);
                _dbContext.SaveChanges();
                TempData["success"] = "Course added successfully!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var course = _dbContext.Courses.Find(id);
            if (course == null)
            {
                TempData["error"] = "Course not found.";
                return RedirectToAction("Index");
            }

            var model = new CoursesDetail
            {
                Id = course.Id,
                CoursesName = course.CoursesName,
                Subject = course.Subject,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoursesDetail model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please enter valid data.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.CoursesName) ||
                string.IsNullOrWhiteSpace(model.Subject) ||
                model.StartDate == default || model.EndDate == default)
            {
                TempData["error"] = "All fields are required.";
                return View(model);
            }

            if (model.StartDate >= model.EndDate)
            {
                TempData["error"] = "Start date must be before the end date.";
                return View(model);
            }

            var course = _dbContext.Courses.Find(model.Id);
            if (course == null)
            {
                TempData["error"] = "Course not found.";
                return RedirectToAction("Index");
            }

            if (_dbContext.Courses.Any(c => c.CoursesName == model.CoursesName && c.Id != model.Id))
            {
                TempData["error"] = "Course name already exists. Please choose a different name.";
                return View(model);
            }

            try
            {
                course.CoursesName = model.CoursesName;
                course.Subject = model.Subject;
                course.StartDate = model.StartDate;
                course.EndDate = model.EndDate;
                course.UpdatedAt = DateTime.Now;

                _dbContext.Courses.Update(course);
                _dbContext.SaveChanges();
                TempData["success"] = "Course updated successfully!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var course = _dbContext.Courses.Find(id);
            if (course == null)
            {
                TempData["error"] = "Course not found.";
                return RedirectToAction("Index");
            }

            try
            {
                _dbContext.Courses.Remove(course);
                _dbContext.SaveChanges();
                TempData["success"] = "Course deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
