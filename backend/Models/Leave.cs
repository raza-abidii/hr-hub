namespace EMSSolution.Models
{
    public class Leave
    {
        
        public int iMasterid { get; set; }
        public string sLeaveCode { get; set; } = string.Empty;
        public string sLeaveType { get; set; } = string.Empty;
        public bool isPaid { get; set; }
        public int iTotalDays { get; set; }
       
    }

    public class LeaveApplication
    {
        public int id { get; set; }
        
        public int iEmployee { get; set; }
        
        
        public DateTime dFromDate { get; set; }
        public DateTime dToDate { get; set; }
        public double fTotalDaysTaken { get; set; }
        public int iLeaveType { get; set; }
        public double fDaysTakenOnLeaveType { get; set; }
        public string sRemarks { get; set; } = string.Empty;
        public bool isfullday { get; set; }
        public int iApproved1 { get; set; } //1 approved, 0 pending, -1 rejected
        public int iApproved2 { get; set; }
        public int iApproved3 { get; set; }
        public string? sApprovedBy1 { get; set; } = string.Empty;
        public string? sApprovedBy2 { get; set; } = string.Empty;
        public string sApprovedBy3 { get; set; } = string.Empty;

        public int iApprovedAuthority1 { get; set; }
        public int iApprovedAuthority2 { get; set; }
        public int iApprovedAuthority3 { get; set; }


        public string LeaveApplicationId1 { get; set; } = string.Empty;
        public string LeaveApplicationId2 { get; set; } = string.Empty;
        public string LeaveApplicationId3 { get; set; } = string.Empty;

        public string ApprovalRemarks1 { get; set; } = string.Empty;
        public string ApprovalRemarks2 { get; set; } = string.Empty;
        public string ApprovalRemarks3 { get; set; } = string.Empty;

        public DateTime LeaveAppliedTimeStamp { get; set; }= DateTime.Now;
        public DateTime LeaveApprovedTimestamp { get; set; } = DateTime.Now;
    }

    public class LeaveUsedModel
    {
        public int LeaveTypeId { get; set; }
        public int UsedDays { get; set; }
    }

    public class LeaveRequestModel
    {
        public int iEmployee { get; set; }
        public DateTime datefrom { get; set; }
        public DateTime dateto { get; set; }
        public int noofdays { get; set; }
        public string reason { get; set; }= string.Empty;   
        public int isfullday { get; set; }
        public List<LeaveUsedModel> leaveUsed { get; set; }

        
    }

    public class  LeaveDetail
    {
        public int iLeaveType { get; set; }
        public string sLeaveName { get; set; } = string.Empty;
        public double iTotalDays { get; set; }
        public double ileaveDaysPermonth { get; set; }
        public double LeaveTakenCurrentMonth { get; set; }
        public double TotalLeaveTaken { get; set; }
        public double Balance { get; set; }

    }
}
