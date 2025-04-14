using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SIMS.Controllers;
using SIMS.Database;
using SIMS.Models;
using System;
using System.Linq;
using Xunit;

namespace TestCoursesProject
{
    public class TestCourses
    {
        private SimDatacontext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SimDatacontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new SimDatacontext(options);
        }

        private void SetupTempData(Controller controller)
        {
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public void AddCourse_Success()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new CoursesController(context);
            SetupTempData(controller);
            controller.ModelState.Clear(); // valid state

            var model = new CoursesDetail
            {
                CoursesName = "SE06301 APDP",
                Subject = "APDP",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120)
            };

            // Act
            var result = controller.Add(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Single(context.Courses);
        }

        [Fact]
        public void AddCourse_Fail_DuplicateName()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Courses.Add(new Courses
            {
                CoursesName = "SE06301 APDP",
                Subject = "APDP",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120),
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();

            var controller = new CoursesController(context);
            SetupTempData(controller);
            controller.ModelState.Clear();

            var model = new CoursesDetail
            {
                CoursesName = "SE06301 APDP",
                Subject = "Advanced APDP",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120)
            };

            // Act
            var result = controller.Add(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SE06301 APDP", model.CoursesName);
            Assert.Equal(1, context.Courses.Count());
        }

        [Fact]
        public void EditCourse_Success()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var course = new Courses
            {
                CoursesName = "SE06301 AD",
                Subject = "AD",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120),
                CreatedAt = DateTime.Now
            };
            context.Courses.Add(course);
            context.SaveChanges();

            var controller = new CoursesController(context);
            SetupTempData(controller);
            controller.ModelState.Clear();

            var updatedModel = new CoursesDetail
            {
                Id = course.Id,
                CoursesName = "SE06301 AD - Updated",
                Subject = "Advanced AD",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120)
            };

            // Act
            var result = controller.Edit(updatedModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            var updatedCourse = context.Courses.Find(course.Id);
            Assert.Equal("SE06301 AD - Updated", updatedCourse.CoursesName);
        }

        [Fact]
        public void EditCourse_Fail_DuplicateName()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Courses.AddRange(
                new Courses
                {
                    CoursesName = "SE06301 DM",
                    Subject = "DM",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(120),
                    CreatedAt = DateTime.Now
                },
                new Courses
                {
                    CoursesName = "SE06301 AD",
                    Subject = "AD",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(120),
                    CreatedAt = DateTime.Now
                }
            );
            context.SaveChanges();

            var controller = new CoursesController(context);
            SetupTempData(controller);
            controller.ModelState.Clear();

            var editModel = new CoursesDetail
            {
                Id = context.Courses.First(c => c.CoursesName == "SE06301 AD").Id,
                CoursesName = "SE06301 DM", // duplicate name
                Subject = "Duplicate DM",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120)
            };

            // Act
            var result = controller.Edit(editModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SE06301 DM", editModel.CoursesName);
            Assert.Equal(2, context.Courses.Count());
        }

        [Fact]
        public void DeleteCourse_Success()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var course = new Courses
            {
                CoursesName = "SE06301 APDP",
                Subject = "APDP",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(120),
                CreatedAt = DateTime.Now
            };
            context.Courses.Add(course);
            context.SaveChanges();

            var controller = new CoursesController(context);
            SetupTempData(controller);

            // Act
            var result = controller.Delete(course.Id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.False(context.Courses.Any(c => c.Id == course.Id));
        }
    }
}
