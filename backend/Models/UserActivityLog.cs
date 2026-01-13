namespace EMSSolution.Models
{
    public class UserActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
    }
}
