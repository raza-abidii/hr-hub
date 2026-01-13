using EMSSolution.DataAccess;
using EMSSolution.Models;

namespace EMSSolution.LoggingService
{
    public class UserActivityLogger : IUserActivityLogger
    {
        private readonly ApplicationDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserActivityLogger(ApplicationDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string userId, string userName, string controller, string action, string description)
        {
            try
            {
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var log = new UserActivityLog
                {
                    UserId = userId,
                    UserName = userName,
                    Controller = controller,
                    Action = action,
                    Description = description,
                    Timestamp = DateTime.Now,
                    IPAddress = ip 
                };

                _context.UserActivityLogs.Add(log);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them to a file or monitoring system)
                Console.WriteLine($"Error logging user activity: {ex.Message}");
                
            }
            finally
            {
                // Optional: Dispose of any resources if needed
            }
        }
    }
}
