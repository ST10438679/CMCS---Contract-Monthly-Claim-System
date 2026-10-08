using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PROG6212_POE.Controllers;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using PROG6212_POE.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PROG6212_POE.Test
{
    public class LecturerControllerTests : IDisposable
    {
        private ClaimService _claimService;
        private CMCSDbContext _dbContext;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private LecturerController _controller;
        private readonly int _testUserId = 1;

        public LecturerControllerTests()
        {
            // Setup in-memory database with unique name for each test
            var options = new DbContextOptionsBuilder<CMCSDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new CMCSDbContext(options);

            // Seed test user
            var testUser = new User
            {
                Id = _testUserId,
                Name = "Test",
                Surname = "User",
                Email = "test@test.com",
                Password = "TestPassword123", // Add required password
                Role = "Lecturer", // Add required role
                HourlyRate = 100,
                HourCap = 40
            };
            _dbContext.Users.Add(testUser);
            _dbContext.SaveChanges();

            // Initialize ClaimService with the context
            _claimService = new ClaimService(_dbContext);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.ContentRootPath).Returns("C:\\fakepath");

            _controller = new LecturerController(_claimService, _mockEnv.Object, _dbContext);

            // Setup session
            var mockSession = new Mock<ISession>();

            // Mock GetInt32 for UserId by mocking TryGetValue
            byte[] userIdBytes = BitConverter.GetBytes(_testUserId);
            mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes)).Returns(true);

            // Mock GetString for UserName
            byte[] userNameBytes = System.Text.Encoding.UTF8.GetBytes("Test User");
            mockSession.Setup(s => s.TryGetValue("UserName", out userNameBytes)).Returns(true);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = mockSession.Object;

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        private void ClearClaims()
        {
            var claims = _dbContext.Claims.ToList();
            _dbContext.Claims.RemoveRange(claims);
            _dbContext.SaveChanges();
        }

        #region Index Tests

        [Fact]
        public void Index_WithoutAuthentication_RedirectsToLogin()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            byte[] outVal = null;
            mockSession.Setup(s => s.TryGetValue("UserId", out outVal)).Returns(false);
            _controller.ControllerContext.HttpContext.Session = mockSession.Object;

            // Act
            var result = _controller.Index() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithEmptyList()
        {
            // Arrange
            ClearClaims();

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(result.Model);
            Assert.Empty(model);
        }

        #endregion

        #region IndexByStatus Tests

        [Fact]
        public void IndexByStatus_WithoutAuthentication_RedirectsToLogin()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            byte[] outVal = null;
            mockSession.Setup(s => s.TryGetValue("UserId", out outVal)).Returns(false);
            _controller.ControllerContext.HttpContext.Session = mockSession.Object;

            // Act
            var result = _controller.IndexByStatus("Pending") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        #endregion

        #region Read Tests

        [Fact]
        public void Read_WithInvalidId_RedirectsToIndex_WithError()
        {
            // Arrange
            ClearClaims();

            // Act
            var result = _controller.Read(999) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        #endregion

        #region Create GET Tests

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_WithValidClaim_RedirectsToIndex()
        {
            // Arrange
            var claim = new Claim
            {
                hoursWorked = 10,
                hourlyRate = 100,
                claimNotes = "Test notes"
            };

            // Act
            var result = await _controller.Create(claim, null) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Success"));
        }

        [Fact]
        public async Task Create_Post_WithZeroHours_ReturnsViewWithModelError()
        {
            // Arrange
            var claim = new Claim { hoursWorked = 0, hourlyRate = 100, claimNotes = "Test" };

            // Act
            var result = await _controller.Create(claim, null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(_controller.ModelState.ContainsKey("hoursWorked"));
        }

        [Fact]
        public async Task Create_Post_WithZeroRate_ReturnsViewWithModelError()
        {
            // Arrange
            var claim = new Claim { hoursWorked = 10, hourlyRate = 0, claimNotes = "Test" };

            // Act
            var result = await _controller.Create(claim, null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(_controller.ModelState.ContainsKey("hourlyRate"));
        }

        [Fact]
        public async Task Create_Post_WithEmptyNotes_ReturnsViewWithModelError()
        {
            // Arrange
            var claim = new Claim { hoursWorked = 10, hourlyRate = 100, claimNotes = "" };

            // Act
            var result = await _controller.Create(claim, null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(_controller.ModelState.ContainsKey("claimNotes"));
        }

        [Fact]
        public async Task Create_Post_WithFileTooLarge_ReturnsViewWithError()
        {
            // Arrange
            var claim = new Claim { hoursWorked = 10, hourlyRate = 100, claimNotes = "Test" };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024);
            mockFile.Setup(f => f.FileName).Returns("test.pdf");

            // Act
            var result = await _controller.Create(claim, mockFile.Object) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(_controller.ModelState.ContainsKey("documentFile"));
        }

        [Fact]
        public async Task Create_Post_WithInvalidFileType_ReturnsViewWithError()
        {
            // Arrange
            var claim = new Claim { hoursWorked = 10, hourlyRate = 100, claimNotes = "Test" };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.exe");

            // Act
            var result = await _controller.Create(claim, mockFile.Object) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(_controller.ModelState.ContainsKey("documentFile"));
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public void Delete_Get_WithInvalidId_RedirectsToIndex()
        {
            // Arrange
            ClearClaims();

            // Act
            var result = _controller.Delete(999) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public void DeleteConfirmed_WithInvalidId_RedirectsToIndex_WithError()
        {
            // Arrange
            ClearClaims();

            // Act
            var result = _controller.DeleteConfirmed(999) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        #endregion

        #region GetClaim Tests

        [Fact]
        public void GetClaim_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            ClearClaims();

            // Act
            var result = _controller.GetClaim(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}