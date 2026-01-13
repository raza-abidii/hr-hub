namespace EMSSolution.LoggingService
{
    public interface IUserActivityLogger
    {
        Task LogAsync(string userId, string userName, string controller, string action, string description);
    }
}
