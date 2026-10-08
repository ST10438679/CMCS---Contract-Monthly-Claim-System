using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using PROG6212_POE.Data;
using Microsoft.EntityFrameworkCore;

namespace PROG6212_POE.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ClaimService _claimService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CMCSDbContext _context;

        private string GetSecureFolder()
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Documents");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        // Helper method to check authentication
        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // Helper method to get current user
        private User GetCurrentUser()
        {
            int userId = GetCurrentUserId();
            return _context.Users.FirstOrDefault(u => u.Id == userId);
        }

        // Helper method to set user ViewBag data
        private void SetUserViewBagData()
        {
            var user = GetCurrentUser();
            if (user != null)
            {
                ViewBag.UserName = user.Name;
                ViewBag.UserSurname = user.Surname;
                ViewBag.UserFullName = $"{user.Name} {user.Surname}";
                ViewBag.UserHourlyRate = user.HourlyRate ?? 0;
                ViewBag.UserHourCap = user.HourCap ?? 0;
            }
        }

        public LecturerController(ClaimService claimService, IWebHostEnvironment webHostEnvironment, CMCSDbContext context)
        {
            _claimService = claimService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Get only claims for the logged-in lecturer
            int userId = GetCurrentUserId();
            var claims = _claimService.GetClaimsByUserId(userId);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(claims);
        }

        [HttpGet]
        public IActionResult IndexByStatus(string status)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = GetCurrentUserId();
            var allClaims = _claimService.GetClaimsByUserId(userId);
            var claims = allClaims.Where(c => c.claimStatus.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            return View("Index", claims);
        }

        [HttpGet]
        public IActionResult Read(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _claimService.GetClaimById(id);

            // Verify the claim belongs to the logged-in user
            if (claim == null || claim.userId != GetCurrentUserId())
            {
                TempData["Error"] = "Claim not found or access denied.";
                return RedirectToAction("Index");
            }

            SetUserViewBagData();
            return View(claim);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var user = GetCurrentUser();
            var claim = new Claim
            {
                userId = GetCurrentUserId(),
                hourlyRate = (int)(user?.HourlyRate ?? 100)
            };

            SetUserViewBagData();
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile documentFile)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Set the userId to the logged-in user
                claim.userId = GetCurrentUserId();
                var user = GetCurrentUser();

                // Set hourly rate from user profile
                if (user != null && user.HourlyRate.HasValue)
                {
                    claim.hourlyRate = (int)user.HourlyRate.Value;
                }

                // Validate hour cap
                if (user != null && user.HourCap.HasValue)
                {
                    if (claim.hoursWorked > user.HourCap.Value)
                    {
                        ModelState.AddModelError("hoursWorked", $"Hours worked cannot exceed your hour cap of {user.HourCap.Value} hours.");
                        SetUserViewBagData();
                        return View(claim);
                    }
                }

                if (documentFile != null && documentFile.Length > 0)
                {
                    // Validate file size (max 10MB)
                    if (documentFile.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("documentFile", "File size cannot exceed 10MB.");
                        SetUserViewBagData();
                        return View(claim);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".xls" };
                    var extension = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("documentFile", "Only PDF, Word, and Excel files are allowed.");
                        SetUserViewBagData();
                        return View(claim);
                    }

                    // Create unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}_{documentFile.FileName}";
                    claim.documentName = uniqueFileName;

                    var securePath = Path.Combine(GetSecureFolder(), uniqueFileName);

                    using (var memoryStream = new MemoryStream())
                    {
                        await documentFile.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();

                        // Encrypt and save
                        var encryptedBytes = EncryptionService.EncryptData(fileBytes);
                        await System.IO.File.WriteAllBytesAsync(securePath, encryptedBytes);

                        claim.document = memoryStream.ToArray();
                    }
                }

                // Validate the claim
                if (claim.hoursWorked <= 0)
                {
                    ModelState.AddModelError("hoursWorked", "Hours worked must be greater than 0.");
                    SetUserViewBagData();
                    return View(claim);
                }

                if (claim.hourlyRate <= 0)
                {
                    ModelState.AddModelError("hourlyRate", "Hourly rate must be greater than 0.");
                    SetUserViewBagData();
                    return View(claim);
                }

                if (string.IsNullOrWhiteSpace(claim.claimNotes))
                {
                    ModelState.AddModelError("claimNotes", "Claim notes are required.");
                    SetUserViewBagData();
                    return View(claim);
                }

                bool success = _claimService.CreateClaim(claim);

                if (success)
                {
                    TempData["Success"] = "Claim created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Failed to create claim. Please try again.";
                    SetUserViewBagData();
                    return View(claim);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                SetUserViewBagData();
                return View(claim);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _claimService.GetClaimById(id);

            // Verify the claim belongs to the logged-in user
            if (claim == null || claim.userId != GetCurrentUserId())
            {
                TempData["Error"] = "Claim not found or access denied.";
                return RedirectToAction("Index");
            }

            SetUserViewBagData();
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var claim = _claimService.GetClaimById(id);

                // Verify the claim belongs to the logged-in user
                if (claim == null || claim.userId != GetCurrentUserId())
                {
                    TempData["Error"] = "Claim not found or access denied.";
                    return RedirectToAction("Index");
                }

                bool success = _claimService.DeleteClaim(id);

                if (success)
                {
                    TempData["Success"] = "Claim deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete claim. Claim not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetClaim(int id)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var claim = _claimService.GetClaimById(id);

            // Verify the claim belongs to the logged-in user
            if (claim == null || claim.userId != GetCurrentUserId())
            {
                return NotFound();
            }

            return Json(claim);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _claimService.GetClaimById(id);

            // Verify the claim belongs to the logged-in user
            if (claim == null || claim.userId != GetCurrentUserId())
            {
                TempData["Error"] = "Document not found or access denied.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(claim.documentName))
            {
                TempData["Error"] = "Document not found.";
                return RedirectToAction("Read", new { id });
            }

            var securePath = Path.Combine(GetSecureFolder(), claim.documentName);

            if (!System.IO.File.Exists(securePath))
            {
                TempData["Error"] = "Document file not found.";
                return RedirectToAction("Read", new { id });
            }

            var encryptedBytes = await System.IO.File.ReadAllBytesAsync(securePath);
            var decryptedBytes = EncryptionService.DecryptData(encryptedBytes);

            // Get original filename (remove GUID prefix)
            var fileName = claim.documentName;
            if (fileName.Length > 36 && fileName[36] == '_')
                fileName = fileName.Substring(37);

            return File(decryptedBytes, "application/octet-stream", fileName);
        }
    }
}