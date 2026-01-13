namespace EMSSolution.Models
{
    public class Preference
    {
        public int id { get; set; }
        public bool secLvlLeaveApproval { get; set; }
        public string? secLvlLeaveAppUser { get; set; }
        public string? secLvlLeaveAppUserMail { get; set; }
        public string? HrEmailId { get; set; }
    }
}
