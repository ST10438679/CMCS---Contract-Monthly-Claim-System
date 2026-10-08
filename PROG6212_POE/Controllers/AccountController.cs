using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Data;
using PROG6212_POE.Models;

namespace PROG6212_POE.Controllers
{
    public class AccountController : Controller
    {
        private readonly CMCSDbContext _context;

        public AccountController(CMCSDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Find user by name and password 
            var user = _context.Users
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.Password == password);

            if (user != null)
            {
                string role = user.Role;

                // Store user information in session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserSurbane", user.Surname);
                HttpContext.Session.SetString("UserRole", user.Role);

                // Redirect based on role
                return role switch
                {
                    "Lecturer" => RedirectToAction("Index", "Lecturer"),
                    "Programme Coordinator" => RedirectToAction("Index", "Coordinator"),
                    "Academic Manager" => RedirectToAction("Index", "Manager"),
                    "HR" => RedirectToAction("Index", "HR"),
                    _ => RedirectToAction("Login", "Account")
                };
            }

            TempData["Error"] = "Invalid login credentials";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}