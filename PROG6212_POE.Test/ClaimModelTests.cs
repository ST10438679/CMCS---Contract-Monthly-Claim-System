using PROG6212_POE.Models;

namespace PROG6212_POE.Test
{
    public class ClaimModelTests
    {
        // Test to verify that totalAmount is calculated correctly
        [Fact]
        public void CaculateTotalAmount()
        {
            var claim = new Claim
            {
                hourlyRate = 150,
                hoursWorked = 8
            };
            var totalAmount = claim.totalAmount;
            Assert.Equal(1200, totalAmount);
        }
    }
}