namespace EMSSolution.Models
{
    public class ExpenseAlert
    {

    }
    public class expenseAlertEmployee
    {
        public long Sno { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public double RequestedAmount { get; set; }
        public double ApprovedAmount { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;


    }

    public class expenseAlertAdmin
    {
        public long Sno { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ExpenseDate { get; set; } = string.Empty;
        public string  Description { get; set; }
        public double RequestedAmount { get; set; }
        public int id { get; set; } 


    }
}
