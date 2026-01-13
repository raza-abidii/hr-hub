namespace EMSSolution.Models
{
    public class EarningDeduction
    {
        public int id  { get; set; }
        public int iType { get; set; } // 0 for Earning, 1 for Deduction
        public string TypeName { get; set; }= string.Empty;

    }
}
