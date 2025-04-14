using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Controllers;
using SIMS.Database;
using SIMS.Models;
using System;
using Xunit;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace SIMS.Tests.Controllers
{
    public class StudentControllerTests
    {
        private SimDatacontext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SimDatacontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new SimDatacontext(options);
        }

        private StudentController CreateControllerWithTempData(SimDatacontext dbContext)
        {
            // Initialize TempData
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            var controller = new StudentController(dbContext)
            {
                TempData = tempData
            };
            return controller;
        }

        [Fact]
        public void Index_ReturnsViewWithStudents()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            dbContext.Student.AddRange(
                new Student { StudentName = "Alice", StudentCode = "001", Email = "alice@test.com", Course = "Math" },
                new Student { StudentName = "Bob", StudentCode = "002", Email = "bob@test.com", Course = "Physics" }
            );
            dbContext.SaveChanges();
            var controller = new StudentController(dbContext);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<StudentViewModel>(viewResult.Model);
            Assert.Equal(2, model.StudentList.Count);
        }

        [Fact]
        public void Add_ValidModel_RedirectsToIndexAndSavesStudent()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = CreateControllerWithTempData(dbContext); // TempData initialized
            var model = new StudentDetail
            {
                Name = "Charlie",
                StudentCode = "003",
                Email = "charlie@test.com",
                Course = "Physics",
                Address = "123 Main St"
            };

            // Act
            var result = controller.Add(model);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Single(dbContext.Student.ToList());
            Assert.Equal("Student added successfully!", controller.TempData["success"]);
        }

        [Fact]
        public void Add_InvalidModel_ReturnsViewWithError()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new StudentController(dbContext);
            var model = new StudentDetail { Name = "", StudentCode = "", Email = "", Course = "" }; // Missing required fields
            controller.ModelState.AddModelError("Name", "Required");
            controller.ModelState.AddModelError("StudentCode", "Required");
            controller.ModelState.AddModelError("Email", "Required");
            controller.ModelState.AddModelError("Course", "Required");

            // Act
            var result = controller.Add(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void Add_DuplicateStudentCode_ShowsError()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            dbContext.Student.Add(new Student
            {
                StudentName = "Existing",
                StudentCode = "004",
                Email = "existing@test.com",
                Course = "Math",
                Address = "456 Oak St"
            });
            dbContext.SaveChanges();
            var controller = new StudentController(dbContext);
            var model = new StudentDetail
            {
                Name = "Dave",
                StudentCode = "004", // Duplicate code
                Email = "new@test.com",
                Course = "Math",
                Address = "789 Pine St"
            };

            // Act
            var result = controller.Add(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Contains("StudentCode", controller.ModelState.Keys);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void Edit_Get_ValidId_ReturnsStudentDetails()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var student = new Student
            {
                Id = 1,
                StudentName = "Eve",
                StudentCode = "005",
                Email = "eve@test.com",
                Course = "Biology",
                Address = "101 Elm St"
            };
            dbContext.Student.Add(student);
            dbContext.SaveChanges();
            var controller = new StudentController(dbContext);

            // Act
            var result = controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudentDetail>(viewResult.Model);
            Assert.Equal("Eve", model.Name);
        }

        [Fact]
        public void Edit_Get_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new StudentController(dbContext);

            // Act
            var result = controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_ValidModel_UpdatesStudent()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var student = new Student
            {
                Id = 1,
                StudentName = "Old Name",
                StudentCode = "006",
                Email = "old@test.com",
                Course = "Chemistry",
                Address = "202 Maple St"
            };
            dbContext.Student.Add(student);
            dbContext.SaveChanges();
            var controller = CreateControllerWithTempData(dbContext); // TempData initialized
            var model = new StudentDetail
            {
                Id = 1,
                Name = "New Name",
                StudentCode = "006",
                Email = "new@test.com",
                Course = "Biology",
                Address = "303 Cedar St"
            };

            // Act
            var result = controller.Edit(model);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var updatedStudent = dbContext.Student.Find(1);
            Assert.Equal("New Name", updatedStudent.StudentName);
            Assert.Equal("Student updated successfully!", controller.TempData["success"]);
        }

        [Fact]
        public void Delete_ValidId_RemovesStudent()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var student = new Student
            {
                Id = 1,
                StudentName = "Test",
                StudentCode = "007",
                Email = "test@test.com",
                Course = "Physics",
                Address = "404 Oak St"
            };
            dbContext.Student.Add(student);
            dbContext.SaveChanges();
            var controller = CreateControllerWithTempData(dbContext); // TempData initialized

            // Act
            var result = controller.Delete(1);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(dbContext.Student.ToList());
            Assert.Equal("Student deleted successfully!", controller.TempData["success"]);
        }


        [Fact]
        public void Delete_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var controller = new StudentController(dbContext);

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}