using System.ComponentModel.DataAnnotations;
namespace EMSSolution.Models
{
    public class EmployeeSalaryData
    {
        public int EmployeeId { get; set; }
        public List<EarnDed> EarnDeduct { get; set; } = new List<EarnDed>();
    }
    public class EarnDed
    {
        public int iType { get; set; } // 0 for earning, 1 for deduction
        public string TypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
    public class EmployeeSalaryModel
    {
        public int id { get; set; }
        public int iEmployeeId { get; set; }
        public int iEarningDeductionType { get; set; }
        public string  EarningDeductionTypeName { get; set; }=string.Empty;
        public double Amount { get; set; }
    }
}
