using Microsoft.AspNetCore.Mvc;
using SIMS.Database;
using Microsoft.AspNetCore.Authorization;

namespace SIMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class DashboardController : Controller
    {

        private readonly SimDatacontext _context;

        public DashboardController(SimDatacontext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy tổng số sinh viên, khóa học, giảng viên từ database
            int totalStudents = _context.Student.Count(); // Đã sửa lỗi DbSet
            int totalCourses = _context.Courses.Count();
            int totalTeachers = _context.Teacher.Count(); // Đã sửa lỗi DbSet

            // Gửi dữ liệu về View để hiển thị
            ViewData["TotalStudents"] = totalStudents;
            ViewData["TotalCourses"] = totalCourses;
            ViewData["TotalTeachers"] = totalTeachers;

            return View();
        }

    }
}