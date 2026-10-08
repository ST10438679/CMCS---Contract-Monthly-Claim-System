using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Data;
using PROG6212_POE.Services;

namespace PROG6212_POE.Controllers
{
    public class HRController : Controller
    {
        private readonly CMCSDbContext _context;
        private readonly ClaimService _claimService;

        private readonly InvoiceService _invoiceService;

        public HRController(CMCSDbContext context, ClaimService claimService, InvoiceService invoiceService)
        {
            _context = context;
            _claimService = claimService;
            _invoiceService = invoiceService;
        }

        // Helper method to check authentication
        private bool IsAuthenticated()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "HR";
        }

        // GET: Index - HR Dashboard
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        // GET: ManageUsers - List all users (excluding HR accounts)
        public IActionResult ManageUsers()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Filter out HR users from the list
            var users = _context.Users
                .Where(u => u.Role != "HR")
                .OrderBy(u => u.Surname)
                .ToList();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(users);
        }

        // GET: CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Prevent HR from creating HR users
                if (user.Role == "HR")
                {
                    ModelState.AddModelError("Role", "You cannot create HR users.");
                    return View(user);
                }

                // Validate Lecturer-specific fields
                if (user.Role == "Lecturer")
                {
                    if (user.HourlyRate == null || user.HourlyRate <= 0)
                    {
                        ModelState.AddModelError("HourlyRate", "Hourly Rate is required for Lecturers and must be greater than 0.");
                        return View(user);
                    }

                    if (user.HourCap == null || user.HourCap <= 0)
                    {
                        ModelState.AddModelError("HourCap", "Hour Cap is required for Lecturers and must be greater than 0.");
                        return View(user);
                    }
                }
                else
                {
                    // Clear Lecturer-specific fields for non-Lecturer roles
                    user.HourlyRate = null;
                    user.HourCap = null;
                }

                // Check if email already exists
                if (_context.Users.Any(u => u.Email.ToLower() == user.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(user);
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    ModelState.AddModelError("Name", "Name is required.");
                    return View(user);
                }

                if (string.IsNullOrWhiteSpace(user.Surname))
                {
                    ModelState.AddModelError("Surname", "Surname is required.");
                    return View(user);
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    ModelState.AddModelError("Email", "Email is required.");
                    return View(user);
                }

                if (string.IsNullOrWhiteSpace(user.Password))
                {
                    ModelState.AddModelError("Password", "Password is required.");
                    return View(user);
                }

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "User created successfully!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(user);
            }
        }

        // GET: EditUser
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            // Prevent editing HR users
            if (user.Role == "HR")
            {
                TempData["Error"] = "You cannot edit HR users.";
                return RedirectToAction("ManageUsers");
            }

            return View(user);
        }

        // POST: EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);

                if (existingUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageUsers");
                }

                // Prevent editing HR users
                if (existingUser.Role == "HR")
                {
                    TempData["Error"] = "You cannot edit HR users.";
                    return RedirectToAction("ManageUsers");
                }

                // Prevent changing role to HR
                if (user.Role == "HR")
                {
                    ModelState.AddModelError("Role", "You cannot change a user's role to HR.");
                    return View(user);
                }

                // Validate Lecturer-specific fields
                if (user.Role == "Lecturer")
                {
                    if (user.HourlyRate == null || user.HourlyRate <= 0)
                    {
                        ModelState.AddModelError("HourlyRate", "Hourly Rate is required for Lecturers and must be greater than 0.");
                        return View(user);
                    }

                    if (user.HourCap == null || user.HourCap <= 0)
                    {
                        ModelState.AddModelError("HourCap", "Hour Cap is required for Lecturers and must be greater than 0.");
                        return View(user);
                    }
                }
                else
                {
                    // Clear Lecturer-specific fields for non-Lecturer roles
                    user.HourlyRate = null;
                    user.HourCap = null;
                }

                // Check if email already exists (excluding current user)
                if (_context.Users.Any(u => u.Email.ToLower() == user.Email.ToLower() && u.Id != user.Id))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(user);
                }

                // Update user details
                existingUser.Name = user.Name;
                existingUser.Surname = user.Surname;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.HourlyRate = user.HourlyRate;
                existingUser.HourCap = user.HourCap;

                // Only update password if a new one is provided
                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                _context.SaveChanges();

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(user);
            }
        }

        // GET: DeleteUser
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            // Prevent deleting HR users
            if (user.Role == "HR")
            {
                TempData["Error"] = "You cannot delete HR users.";
                return RedirectToAction("ManageUsers");
            }

            return View(user);
        }

        // POST: DeleteUserConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserConfirmed(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("ManageUsers");
                }

                // Prevent deleting HR users
                if (user.Role == "HR")
                {
                    TempData["Error"] = "You cannot delete HR users.";
                    return RedirectToAction("ManageUsers");
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                TempData["Success"] = "User deleted successfully!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ManageUsers");
            }
        }

        // GET: Reports - View all claims for reporting
        public IActionResult Reports()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = _claimService.GetAllClaims();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(claims);
        }

        // GET: GenerateInvoice - Generate invoice for a specific claim
        public IActionResult GenerateInvoice(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _claimService.GetClaimById(id);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Reports");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(claim);
        }

        public IActionResult DownloadInvoice(int id)
        {
            var claim = _claimService.GetClaimById(id);
            if (claim == null) return NotFound();

            var pdfBytes = _invoiceService.GenerateInvoicePDF(claim);
            return File(pdfBytes, "application/pdf", $"Invoice-{claim.claimId}.pdf");
        }
    }
}