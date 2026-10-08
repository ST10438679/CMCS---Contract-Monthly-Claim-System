using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE.Models
{
    public class Claim
    {
        // Claim attributes with display names
        [Display(Name = "Claim ID")]
        public int claimId { get; set; }

        [Display(Name = "Hourly Rate")]
        public int hourlyRate { get; set; } = 100;

        [Display(Name = "Hours Worked")]
        public int hoursWorked { get; set; }

        [Display(Name = "Claim Notes")]
        public string claimNotes { get; set; }

        public bool firstApproval { get; set; } = false;

        // Claim status is defaulted to "Pending" when claim is created
        [Display(Name = "Claim Status")]
        public string claimStatus { get; set; } = "Pending";

        // Date of claim is set to current date when claim is created
        [Display(Name = "Date of Claim")]
        public DateTime dateOfClaim { get; set; } = DateTime.Now;

        [Display(Name = "Document Name")]
        public string? documentName { get; set; }

        public byte[]? document { get; set; }

        // Computed property for total amount
        [Display(Name = "Total Amount")]
        public int totalAmount
        {
            get
            {
                return hourlyRate * hoursWorked;
            }
        }

        // Foreign key to associate claim with a user
        public int userId { get; set; } = 3;

        public User theUser { get; set; }
    }
}
