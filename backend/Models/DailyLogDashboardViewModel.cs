using System.ComponentModel;

namespace EMSSolution.Models
{
    public class DailyLogDashboardViewModel
    {
      

        // Data for the table
        public List<LogEntryViewModel> LogEntries { get; set; }

        // Properties for Paging
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
    }

    public class LogEntryViewModel
    {
        [DisplayName("Device ID")]
        public Int32 DeviceId { get; set; }

        [DisplayName("Device Name")]
        public string DeviceName { get; set; }

        [DisplayName("Serial No.")]
        public string SerialNo { get; set; }

        [DisplayName("Emp ID")]
        public string EmpId { get; set; }

        [DisplayName("Employee Name")]
        public string EmpName { get; set; }

        [DisplayName("Log Date")]
        public DateTime LogDate { get; set; }

        [DisplayName("Verify Method")]
        public string VerifyMethod { get; set; }
    }
}
