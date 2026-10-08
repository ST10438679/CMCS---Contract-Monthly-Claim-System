using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Services;

namespace PROG6212_POE.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ClaimService _claimService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManagerController(ClaimService claimService, IWebHostEnvironment webHostEnvironment)
        {
            _claimService = claimService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper method to check authentication
        private bool IsAuthenticated()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Academic Manager" || role == "HR";
        }

        // GET: Index - Display claims that have been approved by Coordinator
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var filteredItems = _claimService.GetClaimsByFirstApproval(true);
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(filteredItems);
        }

        // GET: Read - View claim details
        public IActionResult Read(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _claimService.GetClaimById(id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Index");
            }
            return View(claim);
        }

        // POST: Update Final Approval Status
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var claim = _claimService.GetClaimById(id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction("Index");
                }

                if (status != "Approved" && status != "Rejected")
                {
                    TempData["ErrorMessage"] = "Invalid status value.";
                    return RedirectToAction("Read", new { id = id });
                }

                claim.claimStatus = status;

                bool updateSuccess = _claimService.UpdateClaim(claim);

                if (updateSuccess)
                {
                    TempData["SuccessMessage"] = $"Claim status updated to '{status}' successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update claim status.";
                }

                return RedirectToAction("Read", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the claim status.";
                return RedirectToAction("Read", new { id = id });
            }
        }

        // Download document
        [HttpGet]
        public IActionResult DownloadDocument(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = _claimService.GetClaimById(id);

            if (claim == null || claim.document == null || claim.document.Length == 0)
            {
                TempData["Error"] = "Document not found.";
                return RedirectToAction("Read", new { id });
            }

            string fileName = claim.documentName;

            var securePath = Path.Combine(GetSecureFolder(), claim.documentName);
            var encryptedBytes = System.IO.File.ReadAllBytes(securePath);
            var decryptedBytes = EncryptionService.DecryptData(encryptedBytes);

            return File(decryptedBytes, "application/octet-stream", fileName);
        }

        private string GetSecureFolder()
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Documents");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}