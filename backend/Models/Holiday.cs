namespace EMSSolution.Models
{
    public class Holiday
    {
        public int iMasterid { get; set; }
        public string sHolidayName { get; set; } = string.Empty;
        public string sHolidayCode { get; set; } = string.Empty;

        public DateTime dDate { get; set; }
    }
}
