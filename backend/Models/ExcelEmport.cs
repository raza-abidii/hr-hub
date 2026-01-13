namespace EMSSolution.Models
{
    public class ExcelEmport
    {
    }
    public class ExportRequest
    {
        public List<Dictionary<string, string>> AttendanceData { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
    }

    public class ExportRequestDateRange
    {
        public List<Dictionary<string, string>> AttendanceData { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
    }
}
