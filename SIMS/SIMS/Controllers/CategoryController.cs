using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Helpers;
using SIMS.Databases;
using SIMS.Database;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class CategoryController : Controller
    {
        private readonly SimDatacontext _dbContext;

        public CategoryController(SimDatacontext context)
        {
            _dbContext = context;
        }

        public IActionResult Index()
        {
            var categoryModel = new CategoryViewModel
            {
                CategoryList = _dbContext.Categories.Select(c => new CategoryDetail
                {
                    Id = c.Id,
                    NameCategory = c.NameCategory,
                    Description = c.Description,
                    Avatar = c.Avatar,
                    Status = c.Status,
                    CreateAt = c.CreatedAt,
                    UpdateAt = c.UpdatedAt
                }).ToList()
            };

            ViewData["title"] = "Category";
            return View(categoryModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryDetail());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryDetail model, IFormFile ViewAvatar)
        {
            if (!ModelState.IsValid)
            {
                TempData["save"] = false;
                return View(model);
            }

            try
            {
                string fileAvatar = new UploadFile(ViewAvatar).Upload("images");
                var category = new Categories
                {
                    NameCategory = model.NameCategory,
                    Description = model.Description,
                    Avatar = fileAvatar,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };

                _dbContext.Categories.Add(category);
                _dbContext.SaveChanges();
                TempData["save"] = true;
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["save"] = false;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category == null)
                return NotFound();

            var model = new CategoryDetail
            {
                Id = category.Id,
                NameCategory = category.NameCategory,
                Description = category.Description,
                Avatar = category.Avatar,
                Status = category.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryDetail model, IFormFile ViewAvatar)
        {
            var category = _dbContext.Categories.Find(model.Id);
            if (category == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["save"] = false;
                return View(model);
            }

            try
            {
                if (ViewAvatar != null)
                {
                    category.Avatar = new UploadFile(ViewAvatar).Upload("images");
                }

                category.NameCategory = model.NameCategory;
                category.Description = model.Description;
                category.Status = model.Status;
                category.UpdatedAt = DateTime.Now;

                _dbContext.Categories.Update(category);
                _dbContext.SaveChanges();
                TempData["save"] = true;
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["save"] = false;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(new CategoryDetail
            {
                Id = category.Id,
                NameCategory = category.NameCategory,
                Description = category.Description,
                Avatar = category.Avatar,
                Status = category.Status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category == null)
                return NotFound();

            try
            {
                _dbContext.Categories.Remove(category);
                _dbContext.SaveChanges();
                TempData["save"] = true;
            }
            catch
            {
                TempData["save"] = false;
            }

            return RedirectToAction("Index");
        }
    }
}