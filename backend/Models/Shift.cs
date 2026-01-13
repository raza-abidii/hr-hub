using Microsoft.EntityFrameworkCore;

namespace EMSSolution.Models
{
    public class Shiftdata
    {
        public Shift shiftdata { get; set; } = new Shift();
        public List<ShiftWeekendData> shiftWeekendData { get; set; } = new List<ShiftWeekendData>();
    }
    
    public class ShiftWeekendData
    {
        public int SNo { get; set; }
        public  int iShiftid { get; set; }
        public string sDay { get; set; }= string.Empty;
        public bool week1Selected { get; set; }
        public string week1WeekendType { get; set; } = string.Empty;

        public bool week2Selected { get; set; }
        public string week2WeekendType { get; set; } = string.Empty;
        public bool week3Selected { get; set; }
        public string week3WeekendType { get; set; } = string.Empty;
        public bool week4Selected { get; set; }
        public string week4WeekendType { get; set; } = string.Empty;

        public bool week5Selected { get; set; }
        public string week5WeekendType { get; set; } = string.Empty;
    }
    public class Shift
    {
        public int iMasterid { get; set; }
        public string sShiftCode { get; set; } = string.Empty;
        public string sShiftName { get; set; } = string.Empty;

        public TimeSpan sStartTime { get; set; }
        public TimeSpan sEndTime { get; set; }

        public int iBreakduration { get; set; } = 0;  //in minutes
        public int iAllowlateminute { get; set; } = 0;
        public int iAllowearlyminute { get; set; } = 0;
        public double fWorkingHour { get; set; } = 0.00; //in hours
        public int iMinOTminute { get; set; } = 0;
        public int iMinuteToConsider { get; set; } = 0;

        public double fHalfday { get; set; } = 0;
        public double fFullDay { get; set; } = 0;
    }
}
