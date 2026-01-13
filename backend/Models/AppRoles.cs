using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMSSolution.Models
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";

        public static List<SelectListItem> GetRoleSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = Admin, Text = "Admin" },
                new SelectListItem { Value = Manager, Text = "Manager" },
                new SelectListItem { Value = Employee, Text = "Employee" }
            };
        }
    }

    public enum UserRole
    {
        Admin,
        Manager,
        Employee
    }

    public static class EnumExtensions
    {
        public static List<SelectListItem> ToSelectList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                }).ToList();
        }
    }
}
