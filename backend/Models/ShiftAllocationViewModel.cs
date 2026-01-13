using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMSSolution.Models
{
    public class ShiftAllocationViewModel
    {
        public string AllocationType { get; set; } // "EmployeeWise" or "CategoryWise"
        public string TimeFrameType { get; set; } // "Monthly" or "Yearly"
        public int? SelectedEmployeeId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }

        public List<SelectListItem> Employees { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> Shifts { get; set; }
        public List<int> DaysInMonth { get; set; }
    }

    public class ShiftAllocationViewPostModel
    {
        public string AllocationType { get; set; }
        public int? SelectedEmployeeId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string TimeFrameType { get; set; }
        public int? BulkShiftSelect { get; set; }

        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }


        // Dynamic keys for daily and monthly shifts
        public Dictionary<string, int> ShiftDays { get; set; } = new();
        public Dictionary<string, int> ShiftMonths { get; set; } = new();
    }
}
