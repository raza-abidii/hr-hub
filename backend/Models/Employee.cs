using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace EMSSolution.Models
{
    //create table tblEmployee(Id int identity, iMasterId int Primary Key,
    //sEmployeeName varchar(100),sEmployeeCode varchar(50),
    //dHireDate datetime, sGender varchar(10),iCategory int,iDesignation int
    //,iDepartment int,iShift int,sPanNo varchar(10),sAadharNo varchar(12),
    //sPassportNo varchar(20),bOTStatus bit, bEmployeeResign bit,bAutoShift bit
    //, DOB datetime,iCountry int,iState int,iCity int,sPhoneNo varchar(15),
    //sEmailId varchar(50),sAddress1 varchar(100),sAddress2 varchar(100)
    //,sLandmark varchar(100),sPincode varchar(10),sMaritalStatus varchar(20)
    //,sEmergencyContact varchar(20))
    public class Employee
    {
        public int iMasterid { get; set; }
        public string sEmployeeName { get; set; } = string.Empty;
        public string sEmployeeCode { get; set; } = string.Empty;
        public DateTime dHireDate { get; set; } = DateTime.Now;
        public string sGender { get; set; } = string.Empty;
        public int iCategory { get; set; }
        public int iBranch { get; set; }
        public int iDesignation { get; set; }
        public int iDepartment { get; set; }
        public int iShift { get; set; }

        public int iReportingTo { get; set; } = 0;
        public string sPanNo { get; set; } = string.Empty;
        public string sAadharNo { get; set; } = string.Empty;
        public string sPassportNo { get; set; } = string.Empty;
        public bool bOTStatus { get; set; }
        public bool bEmployeeResign { get; set; }
        public bool bAutoShift { get; set; }

        public bool bPermanent { get; set; }

        public DateTime DOB { get; set; }
        public int iCountry { get; set; }
        public int iState { get; set; }
        public int iCity { get; set; }
        public string sPhoneNo { get; set; } = string.Empty;
        public string sEmailId { get; set; } = string.Empty;
        public string sAddress1 { get; set; } = string.Empty;
        public string sAddress2 { get; set; } = string.Empty;
        public string sLandmark { get; set; } = string.Empty;
        public string sPincode { get; set; } = string.Empty;
        public string sMaritalStatus { get; set; } = string.Empty;
        public string sEmergencyContact { get; set; } = string.Empty;
        public string sImagePath { get; set; } = string.Empty;
        public string sImage { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string AccountNo { get; set; } = string.Empty;

        public string ifscCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        public DateTime dCreatedDate { get; set; }
        public DateTime dModifiedDate { get; set; }



    }
    public class LeaveAllocation
    {
        public int Id { get; set; }
        public int iEmployee { get; set; }
        public int iLeaveType { get; set; }
        public string sLeaveName { get; set; } = string.Empty;
        public int iLeaveDaysPerMonth { get; set; }

    }
    public class SaveEmployeeRequest
    {
        public Employee Employee { get; set; }
        public List<LeaveAllocation> leaveAllocation { get; set; }
    }

    public class employeeDetail
    {
        public string EmpName = string.Empty;
        public string EmpId = string.Empty;
        public string Branch = string.Empty;
        public string Category = string.Empty;
        public string Department = string.Empty;
        public string Designation = string.Empty;
    }

    public class employeeDetailDB
    {
        public string EmpName = string.Empty;
        public string EmpId = string.Empty;
        public string Branch = string.Empty;
        public int BranchId;
        public string Category = string.Empty;
        public string Department = string.Empty;
        public string Designation = string.Empty;
    }

    public class EmployeeMasterReport
    {
        public string sEmployeeName { get; set; } = string.Empty;
        public string sEmployeeCode { get; set; } = string.Empty;
        public int iEmployeeId { get; set; }
        public string Branch   { get; set; } = string.Empty;
        public int iBranchId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;

        public string MobileNo { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
    }

    public class EmployeeLastWeekAbsentPresentChart
    { 
        public string dDate { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int PresentEmployees { get; set; }
        public int AbsentEmployees { get; set; }
    }

    public class EmployeeLastWeekAttendanceChart
    {
        public string login { get; set; }
        public string logout { get; set; }
        public string HourWorked { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    public class employeeInfo
    {
        public string Photo { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string EmpId { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }
}
