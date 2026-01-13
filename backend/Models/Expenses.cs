namespace EMSSolution.Models
{
    public class Expenses
    {
        public  int id { get; set; }
        public int iEmployee { get; set; }
        public string Description { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string sImage { get; set; } = string.Empty;
        public double ApprovedAmount { get; set; }
        public int ApprovalStatus { get; set; } 
        public int ApprovedBy { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.Now;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public DateTime ApprovedDate { get; set; } = DateTime.Now;


    }
}
