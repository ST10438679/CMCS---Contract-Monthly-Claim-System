using Microsoft.EntityFrameworkCore;
using PROG6212_POE.Models;

namespace PROG6212_POE.Data
{
    public class CMCSDbContext : DbContext
    {
        public CMCSDbContext(DbContextOptions<CMCSDbContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Configure the model to be used with the database
        /// and create some default data
        /// </summary>
        protected override void OnModelCreating(ModelBuilder b)
        {
            // Configure Claim entity
            b.Entity<Claim>(e =>
            {
                e.HasKey(c => c.claimId);

                e.Property(c => c.hourlyRate).IsRequired();
                e.Property(c => c.hoursWorked).IsRequired();
                e.Property(c => c.claimNotes).IsRequired();
                e.Property(c => c.claimStatus).IsRequired().HasMaxLength(50);
                e.Property(c => c.documentName).HasMaxLength(500);

                // Configure relationship with User
                e.HasOne(c => c.theUser)
                 .WithMany()
                 .HasForeignKey(c => c.userId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User entity
            b.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Name).IsRequired().HasMaxLength(100);
                e.Property(u => u.Surname).IsRequired().HasMaxLength(100);
                e.Property(u => u.Role).IsRequired().HasMaxLength(50);
                e.Property(u => u.HourlyRate).HasColumnType("decimal(18,2)");
            });

            // Seed default users
            b.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Thomas",
                    Surname = "Williams",
                    Email = "twill@gmail.com",
                    Password = "ThowillMas361",
                    Role = "HR",
                    HourlyRate = null,
                    HourCap = null
                },
                new User
                {
                    Id = 2,
                    Name = "Madeline",
                    Surname = "Smith",
                    Email = "mad@gmail.com",
                    Password = "SmithEline953",
                    Role = "HR",
                    HourlyRate = null,
                    HourCap = null
                },
                new User
                {
                    Id = 3,
                    Name = "John",
                    Surname = "Doe",
                    Email = "doe@gmail.com",
                    Password = "JoeDohn145",
                    Role = "Lecturer",
                    HourlyRate = 150,
                    HourCap = 40
                },
                new User
                {
                    Id = 4,
                    Name = "Sam",
                    Surname = "Sampson",
                    Email = "samson@gmail.com",
                    Password = "SSson528",
                    Role = "Programme Coordinator",
                    HourlyRate = null,
                    HourCap = null
                },
                new User
                {
                    Id = 5,
                    Name = "Alvin",
                    Surname = "Avin",
                    Email = "AA@gmail.com",
                    Password = "vinny216",
                    Role = "Academic Manager",
                    HourlyRate = null,
                    HourCap = null
                }
            );
        }
    }
}