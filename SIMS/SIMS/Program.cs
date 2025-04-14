using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using SIMS.Database;

namespace SIMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // DbContext configuration
            var provider = builder.Services.BuildServiceProvider();
            var configuration = provider.GetRequiredService<IConfiguration>();
            builder.Services.AddDbContext<SimDatacontext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("connection"))
            );

            // Add Authentication services
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index";    // Trang ðãng nh?p
                    options.LogoutPath = "/Login/Logout";  // Trang ðãng xu?t
                    options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect when access is denied
                });

            // Build the app
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Use Authentication and Authorization middleware
            app.UseAuthentication();  // Thêm middleware Authentication
            app.UseAuthorization();   // Thêm middleware Authorization

            // Routing configuration
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}"
            );

            // Run the application
            app.Run();
        }
    }
}
