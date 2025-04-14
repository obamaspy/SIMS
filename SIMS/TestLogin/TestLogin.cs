using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS.Controllers;
using SIMS.Database;
using SIMS.Models;
using Xunit;

namespace TestLogin
{
    public class TestLogin
    {
        // Phương thức để tạo SimDatacontext với cơ sở dữ liệu In-Memory cho test
        private SimDatacontext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SimDatacontext>()
                .UseInMemoryDatabase(databaseName: "LoginTestDb")
                .Options;

            var context = new SimDatacontext(options);

            // Thêm tài khoản mẫu vào cơ sở dữ liệu In-Memory
            context.Accounts.Add(new Accounts
            {
                Id = 1,
                Email = "test@example.com",
                Password = "password123",
                Role = "Admin"
            });

            context.SaveChanges();
            return context;
        }

        // Kiểm tra Login thành công, chuyển hướng đến Dashboard
        [Fact]
        public async Task Login_Success_ReturnsRedirectToDashboard()
        {
            // Arrange: Khởi tạo dữ liệu và controller
            var context = GetInMemoryDbContext();
            var controller = new LoginController(context);
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act: Thực hiện đăng nhập với thông tin đúng
            var result = await controller.Index(model) as RedirectToActionResult;

            // Kiểm tra kết quả trả về, đảm bảo điều hướng đến trang Dashboard
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Dashboard", result.ControllerName);
        }

        // Kiểm tra Login thất bại, thông báo lỗi
        [Fact]
        public async Task Login_Fail_ReturnsViewWithError()
        {
            // Arrange: Khởi tạo dữ liệu và controller với thông tin đăng nhập sai
            var context = GetInMemoryDbContext();
            var controller = new LoginController(context);
            var model = new LoginViewModel
            {
                Email = "wrong@example.com",
                Password = "wrongpassword"
            };

            // Act: Thực hiện đăng nhập với thông tin sai
            var result = await controller.Index(model) as ViewResult;

            // Kiểm tra thông báo lỗi trong ViewData
            Assert.NotNull(result);
            Assert.Equal("Account or password is invalid", controller.ViewData["MessageLogin"]);
        }
    }
}
