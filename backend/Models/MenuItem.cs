namespace EMSSolution.Models
{
    public class MenuItem
    {
        public static List<string> getMenuItem()
        {
            List<string> menuItems = new List<string>
            {
                "Home",
                "Company",
                "Department",
                "Designation",
                "Category",
                "Branch",
                "Shift",
                "Holiday",
                "MachineMap",
                "Leave",
                "Employee",
                "ShiftAllocation",
                "TimesheetUpdate",
                "LeaveApplication",
                "BiometricImport",
                "EmployeeMasterReport",
                "AttendanceReport",
                "AttendanceReportDateRange",
                "AttendanceReportSummary",
                "LeaveStatusReport",
                "Timecard",
                "TimecardAdvance",
                "UserCreation",
                "UserRights",
                "Backup",
                "EmailConfiguration",
                "LeaveApprovalManual",
                "Map",
                "EmployeeSalaryDefinition",
                "EarningDeduction",
                "Expenses",
                "DailylogDashboard",
                "Preference"
            };
            return menuItems;
        }
    }
}
