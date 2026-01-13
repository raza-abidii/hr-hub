using EMSSolution.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace EMSSolution.DataAccess
{
    public class ApplicationDBContext:DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
            
        }
        public DbSet<Models.Category> Categories { get; set; }
        
        public DbSet<Models.Branch> Branches { get; set; }
        public DbSet<Models.Department> Departments { get; set; }
        public DbSet<Models.Designation> Designations { get; set; }

        public DbSet<Models.MachineMap> MachineMaps { get; set; }

        public DbSet<Models.Shift> Shifts { get; set; }

        public DbSet<Models.Holiday> Holidays { get; set; }

        public DbSet<Models.Leave> Leaves  { get; set; }

        public DbSet<Models.Country> Countries  { get; set; }
        public DbSet<Models.State>  States{ get; set; }
        public DbSet<Models.City>  Cities{ get; set; }
        public DbSet<Models.Employee> Employees { get; set; }
        public DbSet<Models.EmployeePermanent> employeePermanents { get; set; }

        public DbSet<Models.ShiftWeekendData>  shiftWeekendDatas { get; set; }
        public DbSet<Models.ShiftAllocation>  shiftAllocations { get; set; }
        public DbSet<Models.EmployeeTimeSheet> EmployeeTimeSheets { get; set; }

        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<Models.EMSUsers> Users { get; set; }

        public DbSet<Models.UserRights> userRights { get; set; }
        public DbSet<Models.LeaveApplication> leaveApplications { get; set; }
        public DbSet<Models.LeaveAllocation>  leaveAllocations { get; set; } 

        public DbSet<Models.EmailConfigurationModel>  emailConfigurationModels { get; set; }

        public DbSet<Models.Company>  companies{ get; set; }

        public DbSet<Models.EmployeeSalaryModel>  employeeSalaries { get; set; }


        public DbSet<Models.EarningDeduction>  earningDeductions { get; set; }

        public DbSet<Models.Expenses> Expenses { get; set; }

        public DbSet<Models.Preference> Preferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Category>().ToTable("tblCategory").HasKey(c => c.iMasterid);
            
            modelBuilder.Entity<Models.Branch>().ToTable("tblBranch").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.Department>().ToTable("tblDepartment").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.Designation>().ToTable("tblDesignation").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.MachineMap>().ToTable("tblMachineMap").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.Shift>().ToTable("tblShiftDefinition").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.Holiday>().ToTable("tblHoliday").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.Leave>().ToTable("tblLeaveDefinition").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.Country>().ToTable("tblCountries");
            modelBuilder.Entity<Models.State>().ToTable("tblState");
            modelBuilder.Entity<Models.City>().ToTable("tblCities");
            modelBuilder.Entity<Models.Employee>().ToTable("tblEmployee").HasKey(c => c.iMasterid);
            modelBuilder.Entity<Models.EmployeePermanent>().ToTable("tblEmployeePermanent").HasKey(c => c.SNo);
            modelBuilder.Entity<Models.ShiftWeekendData>().ToTable("tblShiftwiseWeekend").HasKey(c => c.SNo);
            modelBuilder.Entity<Models.ShiftAllocation>().ToTable("tblShiftAllocation").HasKey(c => c.SNo);
            modelBuilder.Entity<Models.EmployeeTimeSheet>().ToTable("tblEmployeeTimeSheet").HasKey(c => c.SNo);
            
            modelBuilder.Entity<UserActivityLog>().ToTable("UserActivityLog");
            modelBuilder.Entity<Models.EMSUsers>().ToTable("tblUsers").HasKey(c => c.Id);
            modelBuilder.Entity<Models.UserRights>().ToTable("tblUserRights").HasKey(c => c.Id);
            modelBuilder.Entity<Models.LeaveApplication>().ToTable("tblLeaveApplication").HasKey(c => c.id);
            modelBuilder.Entity<Models.LeaveAllocation>().ToTable("tblLeaveAllocation").HasKey(c => c.Id);

            modelBuilder.Entity<Models.EmailConfigurationModel>().ToTable("tblMailConfiguration").HasKey(c=>c.id);
            modelBuilder.Entity<Models.Company>().ToTable("tblCompany").HasKey(c => c.Id);

            modelBuilder.Entity<Models.EmployeeSalaryModel>().ToTable("tblEmployeeSalaryDefinition").HasKey(c => c.id);
            modelBuilder.Entity<Models.EarningDeduction>().ToTable("tblEarningDeductionMaster").HasKey(c => c.id);
            modelBuilder.Entity<Models.Expenses>().ToTable("tblExpenses").HasKey(c => c.id);

            modelBuilder.Entity<Models.Preference>().ToTable("tblPreference").HasKey(c => c.id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
