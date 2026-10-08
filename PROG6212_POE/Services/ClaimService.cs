using PROG6212_POE.Models;
using Microsoft.EntityFrameworkCore;
using PROG6212_POE.Data;

namespace PROG6212_POE.Services
{
    public class ClaimService
    {
        // Our Db Context readonly file
        private readonly CMCSDbContext _context;

        // Default Constructor
        public ClaimService(CMCSDbContext context)
        {
            _context = context;
        }

        // Get all claims
        // To display our claims usually for index views
        public List<Claim> GetAllClaims()
        {
            return _context.Claims.Include(c => c.theUser).ToList();
        }

        // Get claims by user ID (for lecturers to see only their claims)
        public List<Claim> GetClaimsByUserId(int userId)
        {
            return _context.Claims
                .Include(c => c.theUser)
                .Where(c => c.userId == userId)
                .ToList();
        }

        // Get claims by first approval status
        // The first approval is for our Coordinator as our Manager gets the final say
        public List<Claim> GetClaimsByFirstApproval(bool isApproved)
        {
            return _context.Claims
                .Include(c => c.theUser)
                .Where(c => c.firstApproval == isApproved)
                .ToList();
        }

        // Get claim by ID
        public Claim GetClaimById(int id)
        {
            return _context.Claims
                .Include(c => c.theUser)
                .FirstOrDefault(c => c.claimId == id);
        }

        // Create new claim
        public bool CreateClaim(Claim claim)
        {
            _context.Database.EnsureCreated();

            try
            {
                _context.Claims.Add(claim);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Updating first approval status method
        public bool UpdateFirstApproval(int id, bool approvalStatus)
        {
            try
            {
                var claim = _context.Claims.FirstOrDefault(c => c.claimId == id);
                if (claim != null)
                {
                    claim.firstApproval = approvalStatus;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Update existing claim
        // This is so our claim we pass through here overrides our database claim
        public bool UpdateClaim(Claim claim)
        {
            try
            {
                var existingClaim = _context.Claims.FirstOrDefault(c => c.claimId == claim.claimId);
                if (existingClaim != null)
                {
                    existingClaim.hourlyRate = claim.hourlyRate;
                    existingClaim.hoursWorked = claim.hoursWorked;
                    existingClaim.claimNotes = claim.claimNotes;
                    existingClaim.claimStatus = claim.claimStatus;
                    existingClaim.firstApproval = claim.firstApproval;
                    existingClaim.document = claim.document;
                    existingClaim.documentName = claim.documentName;
                    existingClaim.userId = claim.userId;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Delete claim
        public bool DeleteClaim(int id)
        {
            try
            {
                var claim = _context.Claims.FirstOrDefault(c => c.claimId == id);
                if (claim != null)
                {
                    _context.Claims.Remove(claim);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Get claims by status
        // This is for our filter tabs
        public List<Claim> GetClaimsByStatus(string status)
        {
            return _context.Claims
                .Include(c => c.theUser)
                .Where(c => c.claimStatus.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Get total amount for all claims
        // So we can tally the amount for HR
        public decimal GetTotalAmount()
        {
            return _context.Claims.Sum(c => (decimal)c.totalAmount);
        }

        // Clear all claims (for testing purposes)
        public void ClearClaims()
        {
            _context.Claims.RemoveRange(_context.Claims);
            _context.SaveChanges();
        }
    }
}