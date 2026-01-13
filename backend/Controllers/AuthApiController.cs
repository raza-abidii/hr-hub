using EMSSolution.DataAccess;
using EMSSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using BCrypt.Net;

namespace EMSSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private readonly IConfiguration _configuration;

        public AuthApiController(ApplicationDBContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // Find user by email
                var user = await _db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.EmailId == request.Email);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Verify password using bcrypt
                if (!VerifyPassword(request.Password, user.Password))
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Create session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Email", user.EmailId);
                HttpContext.Session.SetString("IsAuthenticated", "true");

                // Get user role
                var userRights = await _db.userRights
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ur => ur.Id == user.Id);

                var role = userRights?.Role == "Admin" ? "admin" : "employee";

                // Get employee details if available
                var employee = await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.iMasterid == user.EmployeeId);

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id.ToString(),
                        name = $"{user.FirstName} {user.LastName}".Trim(),
                        email = user.EmailId,
                        role = role,
                        department = employee?.iDepartment,
                        designation = employee?.iDesignation,
                        avatar = user.UserImage
                    },
                    token = GenerateAuthToken(user.Id)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Logout failed", error = ex.Message });
            }
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return Unauthorized(new { message = "Invalid session" });
                }

                var user = await _db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == parsedUserId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userRights = await _db.userRights
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ur => ur.Id == user.Id);

                var employee = await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.iMasterid == user.EmployeeId);

                var role = userRights?.Role == "Admin" ? "admin" : "employee";

                return Ok(new
                {
                    id = user.Id.ToString(),
                    name = $"{user.FirstName} {user.LastName}".Trim(),
                    email = user.EmailId,
                    role = role,
                    department = employee?.iDepartment,
                    designation = employee?.iDesignation,
                    avatar = user.UserImage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching current user", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifies password against bcrypt hash or plain text (for backward compatibility)
        /// </summary>
        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
                return false;

            try
            {
                // Try bcrypt verification first (for hashed passwords)
                if (storedPassword.StartsWith("$2")) // bcrypt hash format
                {
                    return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
                }
                // Fallback to plain text comparison for legacy passwords
                return inputPassword == storedPassword;
            }
            catch
            {
                // If bcrypt verification fails, try plain text comparison
                return inputPassword == storedPassword;
            }
        }

        /// <summary>
        /// Hash password using bcrypt
        /// </summary>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private string GenerateAuthToken(int userId)
        {
            // TODO: Implement JWT token generation
            // For now, return a simple token
            var tokenData = $"{userId}:{DateTime.UtcNow.Ticks}";
            var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
