namespace EMSSolution.Models
{
    public class EMSUsers
    {
        public int Id { get; set; } 
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? Salt { get; set; }
        public string? Role { get; set; }
        public int? EmployeeId { get; set; }
        public int iEmployee { get; set; }
        public string UserImage { get; set; } = string.Empty;
        public string? sImage { get; set; }
        public string? iBranchList { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
