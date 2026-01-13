namespace EMSSolution.Models
{
    public class ShiftAllocation
    {
        public int SNo { get; set; }
        public string AllocationType { get; set; } = string.Empty;
        public int iCategory { get; set; }
        public int iEmployee { get; set; }
        public string TimeFrame { get; set; } = string.Empty;
        public int iMonth { get; set; }
        public int iYear { get; set; }
        public int iDay { get; set; }
        public DateTime dDate { get; set; } = DateTime.Now;
        public int iShift { get; set; }


    }
}
