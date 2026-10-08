using System.ComponentModel.DataAnnotations;

public class User : IValidatableObject
{
    // User Attributes
    public int Id { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Role { get; set; }

    public decimal? HourlyRate { get; set; }

    public int? HourCap { get; set; }

    //Method to help determine when hour rate and hour cap are neccessary ie when they are Lecturer
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Role == "Lecturer")
        {
            if (HourlyRate == null || HourlyRate <= 0)
            {
                yield return new ValidationResult(
                    "Hourly Rate is required for Lecturers.",
                    new[] { nameof(HourlyRate) }
                );
            }

            if (HourCap == null || HourCap <= 0)
            {
                yield return new ValidationResult(
                    "Hour Cap is required for Lecturers.",
                    new[] { nameof(HourCap) }
                );
            }
        }
    }
}
