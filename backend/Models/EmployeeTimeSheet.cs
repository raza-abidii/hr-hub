namespace EMSSolution.Models
{
    public class EmployeeTimeSheet
    {
        public int SNo { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string EmpId { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string LogDate { get; set; } = string.Empty;
        public string LogTime { get; set; } = string.Empty;
        public DateTime LogDateTime { get; set; } = DateTime.Now;

    }
}
