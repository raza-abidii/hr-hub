namespace EMSSolution.Models
{
    public class Timecard
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? LogDate { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public TimeSpan? HoursWorked { get; set; }
        public string status { get; set; } = string.Empty;
    }

    public class TimecardAdvance
    {
       
        public string? LogDate { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public TimeSpan? HoursWorked { get; set; }
        public string? Remakrs { get; set; }
    }
}
