namespace EMSSolution.Models
{
    public class ReportModel
    {
    }
    public class AttendanceReportModel
    {
        public int eMasterid { get; set; }
        public string sEmployeeCode { get; set; }
        public string sEmployeeName { get; set; }

        public string sBranch { get; set; }
        public int iBranch { get; set; }

        public string sCategory { get; set; }
        public string sDepartment { get; set; }
        public string sDesignation { get; set; }

        public string LogDate { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }

        public string ShiftType { get; set; }
        public string Status { get; set; }

    }

    public class AttendanceDashboardModel
    {
        public int eMasterid { get; set; }
        public string sEmployeeCode { get; set; } = string.Empty;
        public string sEmployeeName { get; set; } = string.Empty;
        public string sBranch { get; set; } = string.Empty;

        public int iBranch { get; set; }
        public string sCategory { get; set; } = string.Empty;
        public string sDepartment { get; set; } = string.Empty;
        public string sDesignation { get; set; } = string.Empty;

        public string LogDate { get; set; } = string.Empty;
        public DateTime InTime { get; set; } = DateTime.MinValue;
        public DateTime OutTime { get; set; } = DateTime.MinValue; 

        public DateTime sInTime { get; set; } = DateTime.MinValue;

        public DateTime sOutTime { get; set; }=    DateTime.MinValue;
        public string loginStatus { get; set; } = string.Empty;

        public string LateINEarlyOut { get; set; } = string.Empty;
    }

    public class ShiftTimeResult
    {
        public DateTime SinTime { get; set; }
        public DateTime SoutTime { get; set; }
    }

    public class LeaveResponse
    {
        public double LeaveTaken { get; set; }
        
    }
}
