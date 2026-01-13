namespace EMSSolution.Models
{
    public class LeaveApprovalModel
    {
        public int LeaveId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string Remarks { get; set; }
        public string ReportingTo { get; set; }
        public string dDate { get; set; }

    }

    public class LeaveStatusReport
    {
        public int Leaveid { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string LeaveDate { get; set; }
        public string LeaveType { get; set; }
        public string Reason { get; set; }
        public string ApprovalAuthority { get; set; }
        public string Status { get; set; }
       
        public string ApprovedRejectedBy { get; set; }

        public string? AppRejReason { get; set; }
    }
}