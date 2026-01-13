using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using EMSSolution.DataAccess;
using EMSSolution.DatabaseAccessLayer;
using EMSSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Data;
using System.Net.Http;
using System.Security.Principal;
using static ClosedXML.Excel.XLPredefinedFormat;
namespace EMSSolution.GenericMethods
{
    public class TableCreation
    {
        private readonly ApplicationDBContext _db;
        private readonly string database = "";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TableCreation(ApplicationDBContext db, string dBase)
        {
            try
            {
                _db = db;
                //_httpContextAccessor = httpContextAccessor;
                database = dBase;// _httpContextAccessor.HttpContext.Session.GetString("Database");
            }
            catch (Exception ex)
            {

            }
        }

        public void CreateTable()
        {
            try
            {
                CreateTables();
                UpdateTable();
                CreateSPandFunctions();
            }
            catch (Exception ex)
            {
            }
        }
        private void CreateSPandFunctions()
        {
            string strQry;
            string strErrMessage = string.Empty;
            DataSet ds;
            DataLayer dal = new();
            try
            {
                strQry = "Select * from sysObjects where name='sp_AttendanceReport'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Procedure sp_AttendanceReport         
                        @iEmployee int,        
                        @iCategory int,        
                        @iMonth int,        
                        @iYear int,      
                        @reportType varchar(20)      
                        as        
                        begin        
                        if (@reportType='EmployeeWise')      
                        begin      
                        select t.* from (  
                        select distinct e.iMasterId eMasterid,        
                        e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment        
                        ,desig.sName sDesignation,LogDate        
                        ,(select min(es1.logdatetime) from   tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) InTime        
                        ,        
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) OutTime   ,'Present' status      
                        from tblEmployee e join tblcategory c on c.iMasterId=e.iCategory        
                        join tblDepartment d on d.iMasterid=e.iDepartment        
                        join tblDesignation desig on desig.iMasterid=e.iDesignation        
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        where e.iMasterid=@iEmployee        
                        and MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear    
                        )t  
                        union all  
                        select e.iMasterId eMasterid,e.sEmployeeName sEmployeeName,e.sEmployeeCode sEmployeeCode,  
                        c.sName sCategory,d.sname sDepartment,desig.Sname sDesignation,  
                        format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,    
                        case when sLeaveCode='CL' then 'LeaveCL' when sLeaveCode='EL' then 'LeaveEL' else 'Leave' end status     
                        from tblLeaveApplication leave join tblEmployee e    
                        on e.iMasterId=leave.iEmployee    
                        join tblCategory c on c.iMasterId=e.iCategory  
                        join tblDepartment d on d.iMasterId=e.iDepartment  
                        join tblDesignation desig on desig.iMasterid=e.iDesignation  
                        join tblLeaveDefinition ld on ld.iMasterid=leave.iLeaveType  
                        where leave.iEmployee=@iEmployee and month(leave.dFromDate)=@iMonth  
                        and year(leave.dfromDate)=@iYear  
                        order by t.eMasterId      
                        end        
                        else      
                        begin      
                        select t.* from (  
                        select distinct e.iMasterId eMasterid,        
                        e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment        
                        ,desig.sName sDesignation,LogDate        
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) InTime        
                        ,        
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) OutTime,'Present' status      
                        from tblEmployee e join tblcategory c on c.iMasterId=e.iCategory        
                        join tblDepartment d on d.iMasterid=e.iDepartment        
                        join tblDesignation desig on desig.iMasterid=e.iDesignation        
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        where c.iMasterId=@iCategory      
                        and MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear    
                        )t  
                        union all  
                        select e.iMasterId eMasterid,e.sEmployeeName sEmployeeName,e.sEmployeeCode sEmployeeCode,  
                        c.sName sCategory,d.sname sDepartment,desig.Sname sDesignation,  
                        format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,    
                        case when sLeaveCode='CL' then 'LeaveCL' when sLeaveCode='EL' then 'LeaveEL' else 'Leave' end status  
                        from tblLeaveApplication leave join tblEmployee e    
                        on e.iMasterId=leave.iEmployee    
                        join tblCategory c on c.iMasterId=e.iCategory  
                        join tblDepartment d on d.iMasterId=e.iDepartment  
                        join tblDesignation desig on desig.iMasterid=e.iDesignation  
                        join tblLeaveDefinition ld on ld.iMasterid=leave.iLeaveType  
                        where c.iMasterId=@iCategory  and month(leave.dFromDate)=@iMonth   
                        and year(leave.dfromDate)=@iYear    
                        order by t.eMasterId        
                        end      
                        end ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }


                strQry = "Select * from sysObjects where name='sp_AttendanceReportMothStartDay'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Procedure sp_AttendanceReportMothStartDay                    
                        @iEmployee int,                    
                        @iBranch int,                    
                        @fromDate varchar(10),                    
                        @toDate varchar(10),                  
                        @reportType varchar(20)                  
                        as                    
                        begin                    
                        if (@reportType='EmployeeWise')                  
                        begin                  
                        select t.* from (              
                        select distinct e.iMasterId eMasterid,                    
                        e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment                    
                        ,desig.sName sDesignation,LogDate                    
                        ,(select min(es1.logdatetime) from   tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                        group by es1.EmpId,es1.LogDate) InTime                    
                        ,                    
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                        group by es1.EmpId,es1.LogDate) OutTime     
                        ,'Present' status   ,'' ShiftType ,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch              
                        from tblEmployee e left join tblcategory c on c.iMasterId=e.iCategory                    
                        left join tblDepartment d on d.iMasterid=e.iDepartment                    
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation                    
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode     
                        left join tblBranch b on b.iMasterid=e.iBranch
                        where e.iMasterid=@iEmployee                    
                        and cast(logdatetime as date) between @fromDate and @toDate          
                        )t              
                        union all              
                        select e.iMasterId eMasterid,e.sEmployeeName sEmployeeName,e.sEmployeeCode sEmployeeCode,              
                        c.sName sCategory,d.sname sDepartment,desig.Sname sDesignation,              
                        format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,                
                        case when sLeaveCode='CL' then 'LeaveCL' when sLeaveCode='EL' then 'LeaveEL' else 'Leave' end status 
                        ,'' ShiftType,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch 
                        from tblLeaveApplication leave join tblEmployee e                
                        on e.iMasterId=leave.iEmployee                
                        left join tblCategory c on c.iMasterId=e.iCategory              
                        left join tblDepartment d on d.iMasterId=e.iDepartment              
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation              
                        left join tblLeaveDefinition ld on ld.iMasterid=leave.iLeaveType     
                        left join tblBranch b on b.iMasterid=e.iBranch
                        where leave.iEmployee=@iEmployee        
                        and  cast(dFromDate as date) between @fromDate and @toDate          
                        order by t.eMasterId,InTime                  
                        end                    
                        else                  
                        begin                  
                        select t.* from (              
                        select distinct e.iMasterId eMasterid,                    
                        e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment                    
                        ,desig.sName sDesignation,LogDate                    
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                        group by es1.EmpId,es1.LogDate) InTime                    
                        ,                    
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                        group by es1.EmpId,es1.LogDate) OutTime,'Present' status ,'' ShiftType   
                        ,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch 
                        from tblEmployee e left join tblcategory c on c.iMasterId=e.iCategory                    
                        left join tblDepartment d on d.iMasterid=e.iDepartment                    
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation                    
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode     
                        left join tblBranch b on b.iMasterid=e.iBranch
                        where b.iMasterId=@iBranch        
                        and cast(logdatetime as date) between @fromDate and @toDate          
                        )t              
                        union all              
                        select e.iMasterId eMasterid,e.sEmployeeName sEmployeeName,e.sEmployeeCode sEmployeeCode,              
                        c.sName sCategory,d.sname sDepartment,desig.Sname sDesignation,              
                        format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,                
                        case when sLeaveCode='CL' then 'LeaveCL' when sLeaveCode='EL' then 'LeaveEL' else 'Leave' end status    
                        ,'' ShiftType
                        ,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch 
                        from tblLeaveApplication leave join tblEmployee e                
                        on e.iMasterId=leave.iEmployee                
                        left join tblCategory c on c.iMasterId=e.iCategory              
                        left join tblDepartment d on d.iMasterId=e.iDepartment              
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation              
                        left join tblLeaveDefinition ld on ld.iMasterid=leave.iLeaveType    
                        left join tblBranch b on b.iMasterid=e.iBranch
                        where b.iMasterId=@iBranch         
                        and cast(dFromDate as date) between @fromDate and @toDate          
                        order by t.eMasterId ,t.InTime                  
                        end                  
                        end  ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                else
                {
                    strQry = $@"Alter Procedure sp_AttendanceReportMothStartDay                    
                         @iEmployee int,                    
                         @iBranch int,                    
                         @fromDate varchar(10),                    
                         @toDate varchar(10),                  
                         @reportType varchar(20)                  
                         as                    
                         begin                    
                         if (@reportType='EmployeeWise')                  
                         begin                  
                         select t.* from (              
                         select distinct e.iMasterId eMasterid,                    
                         e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment                    
                         ,desig.sName sDesignation,LogDate                    
                         ,(select min(es1.logdatetime) from   tblEmployeeTimeSheet es1 where                     
                         es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                         group by es1.EmpId,es1.LogDate) InTime                    
                         ,                    
                         (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                         es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                         group by es1.EmpId,es1.LogDate) OutTime     
                         ,'Present' status   ,'' ShiftType ,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch              
                         from tblEmployee e left join tblcategory c on c.iMasterId=e.iCategory                    
                         left join tblDepartment d on d.iMasterid=e.iDepartment                    
                         left join tblDesignation desig on desig.iMasterid=e.iDesignation                    
                         join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode     
                         left join tblBranch b on b.iMasterid=e.iBranch
                         where e.iMasterid=@iEmployee                    
                         and cast(logdatetime as date) between @fromDate and @toDate          
                         )t              
                         union all              
                         select e.iMasterId eMasterid,e.sEmployeeName sEmployeeName,e.sEmployeeCode sEmployeeCode,              
                         c.sName sCategory,d.sname sDepartment,desig.Sname sDesignation,              
                         format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,                
                         case when sLeaveCode='CL' then 'LeaveCL' when sLeaveCode='EL' then 'LeaveEL' else 'Leave' end status 
                         ,'' ShiftType,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch 
                         from tblLeaveApplication leave join tblEmployee e                
                         on e.iMasterId=leave.iEmployee                
                         left join tblCategory c on c.iMasterId=e.iCategory              
                         left join tblDepartment d on d.iMasterId=e.iDepartment              
                         left join tblDesignation desig on desig.iMasterid=e.iDesignation              
                         left join tblLeaveDefinition ld on ld.iMasterid=leave.iLeaveType     
                         left join tblBranch b on b.iMasterid=e.iBranch
                         where leave.iEmployee=@iEmployee        
                         and  cast(dFromDate as date) between @fromDate and @toDate          
                         order by t.eMasterId,InTime                  
                         end                    
                         else                  
                         begin                  
                         select t.* from (              
                         select distinct e.iMasterId eMasterid,                    
                         e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment                    
                         ,desig.sName sDesignation,LogDate                    
                         ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                         es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                         group by es1.EmpId,es1.LogDate) InTime                    
                         ,                    
                         (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                         es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                         group by es1.EmpId,es1.LogDate) OutTime,'Present' status ,'' ShiftType   
                         ,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch 
                         from tblEmployee e left join tblcategory c on c.iMasterId=e.iCategory                    
                         left join tblDepartment d on d.iMasterid=e.iDepartment                    
                         left join tblDesignation desig on desig.iMasterid=e.iDesignation                    
                         join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode     
                         left join tblBranch b on b.iMasterid=e.iBranch
                         where b.iMasterId=@iBranch        
                         and cast(logdatetime as date) between @fromDate and @toDate          
                         )t              
                         union all              
                         select e.iMasterId eMasterid,e.sEmployeeName sEmployeeName,e.sEmployeeCode sEmployeeCode,              
                         c.sName sCategory,d.sname sDepartment,desig.Sname sDesignation,              
                         format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,                
                         case when sLeaveCode='CL' then 'LeaveCL' when sLeaveCode='EL' then 'LeaveEL' else 'Leave' end status    
                         ,'' ShiftType
                         ,isnull(b.sName,'') sBranch,isnull(b.iMasterid,0) iBranch 
                         from tblLeaveApplication leave join tblEmployee e                
                         on e.iMasterId=leave.iEmployee                
                         left join tblCategory c on c.iMasterId=e.iCategory              
                         left join tblDepartment d on d.iMasterId=e.iDepartment              
                         left join tblDesignation desig on desig.iMasterid=e.iDesignation              
                         left join tblLeaveDefinition ld on ld.iMasterid=leave.iLeaveType    
                         left join tblBranch b on b.iMasterid=e.iBranch
                         where b.iMasterId=@iBranch         
                         and cast(dFromDate as date) between @fromDate and @toDate          
                         order by t.eMasterId ,t.InTime                  
                         end                  
                         end ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "Select * from sysObjects where name='sp_AttendanceDashBoard'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE Procedure sp_AttendanceDashBoard         
                        @iday int,    
                        @iMonth int,        
                        @iYear int    
                        as        
                        begin        
                        select distinct * from (    
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time) 
                        then 'Ontime' else 'Late' end loginStatus from (    
                        select e.iMasterId eMasterid,        
                        e.sEmployeeName,e.sEmployeeCode ,c.sName sCategory,d.sName sDepartment        
                        ,desig.sName sDesignation,LogDate        
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) InTime        
                        ,        
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) OutTime ,    
                        isnull((select cast(cast(SinTime as time) as datetime) sintime    
                        from (    
                        SELECT
                        DATEADD(minute, s.iAllowlateminute, s.sStartTime) AS SinTime,    
                        DATEADD(minute, s.iAllowearlyminute, s.sEndTime) AS SoutTime    
                        FROM tblShiftDefinition s    
                        JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift    
                        WHERE sa.iDay = @iday AND sa.iMonth = @iMonth    
                        AND sa.iYear = @iYear    
                        AND sa.iEmployee = e.iMasterId     
                        )t),'1900-01-01 00:00:00.000') sintime    
                        from tblEmployee e join tblcategory c on c.iMasterId=e.iCategory        
                        join tblDepartment d on d.iMasterid=e.iDepartment        
                        join tblDesignation desig on desig.iMasterid=e.iDesignation        
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        where    
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear    
                        and day(logdatetime)=@iday    
                        ) t1    
                        )tfinal order by InTime asc      
                        end ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                else
                {
                    strQry = $@"Alter Procedure sp_AttendanceDashBoard         
                        @iday int,    
                        @iMonth int,        
                        @iYear int    
                        as        
                        begin        
                        select distinct * from (    
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time) 
                        then 'Ontime' else 'Late' end loginStatus from (    
                        select e.iMasterId eMasterid,        
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,c.sName sCategory,d.sName sDepartment        
                        ,desig.sName sDesignation,LogDate        
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) InTime        
                        ,        
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where         
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate        
                        group by es1.EmpId,es1.LogDate) OutTime ,    
                        isnull((select cast(cast(SinTime as time) as datetime) sintime    
                        from (    
                        SELECT top 1
                        DATEADD(minute, s.iAllowlateminute, s.sStartTime) AS SinTime,    
                        DATEADD(minute, s.iAllowearlyminute, s.sEndTime) AS SoutTime    
                        FROM tblShiftDefinition s    
                        JOIN tblShiftAllocation sa ON s.iMasterid = sa.iShift    
                        WHERE sa.iDay = @iday AND sa.iMonth = @iMonth    
                        AND sa.iYear = @iYear    
                        AND sa.iEmployee = e.iMasterId     
                        )t),'1900-01-01 00:00:00.000') sintime    
                        from tblEmployee e join tblcategory c on c.iMasterId=e.iCategory        
                        join tblDepartment d on d.iMasterid=e.iDepartment        
                        join tblDesignation desig on desig.iMasterid=e.iDesignation        
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode    
                        left join tblBranch br on br.iMasterid=e.iBranch
                        where    
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear    
                        and day(logdatetime)=@iday    
                        ) t1    
                        )tfinal order by InTime asc      
                        end ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }


                strQry = "Select * from sysObjects where name='sp_DailyAttendanceDashBoard'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Proc sp_DailyAttendanceDashBoard  
                        @iday int,        
                        @iMonth int,            
                        @iYear int        
                        as   
                        begin  
                        --Day Shift        
                         
                        select distinct * from (        
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)     
                        then 'Ontime' else 'Late' end loginStatus   
                        ,case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)   
                        then 'Late-In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)  
                        then 'Early Out' when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and   
                            CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)   
                            then 'Late-In and Early Out'  
                        else ''  
                        end LateINEarlyOut   
  
                        from (        
                        select e.iMasterId eMasterid,            
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,
                        isnull(br.iMasterid,0) iBranch,
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment            
                        ,isnull(desig.sName,'') sDesignation,LogDate            
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate            
                        group by es1.EmpId,es1.LogDate) InTime            
                        ,            
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate            
                        group by es1.EmpId,es1.LogDate) OutTime       
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime) sInTime,  
      
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime) as datetime) sOutTime    
                        ,'Day' ShiftType  
                        from tblEmployee e   
                        left join tblcategory c on c.iMasterId=e.iCategory            
                        left join tblDepartment d on d.iMasterid=e.iDepartment            
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation            
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        left join tblBranch br on br.iMasterid=e.iBranch    
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee   
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift  
                        where        
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear  
                        and day(logdatetime)=@iday      
                        and sa1.iShift in(  
                        select shiftID ShiftType from (  
                        select iMasterid shiftID, sStartTime,sEndTime,  
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,  
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate  
                        from tblShiftDefinition  
                        ) t where DATEDIFF(day,startdate,enddate)<=0  
                        )  
                        ) t1        
                        )tfinal   
  
                        union all  
                        --Night shift  
  
                        select distinct * from (        
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)     
                        then 'Ontime' else 'Late' end loginStatus,   
                        case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)   
                        then 'Late-In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)  
                        then 'Early Out'   
                        when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and   
                        CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)   
                        then 'Late-In and Early Out'  
                        else ''  
                        end LateINEarlyOut  
                        from (        
                        select e.iMasterId eMasterid,            
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch, 
                        isnull(br.iMasterid,0) iBranch,
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment            
                        ,isnull(desig.sName,'') sDesignation,LogDate            
                        ,  
                        isnull((select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and   
                        MONTH(es1.logdatetime)=@iMonth and year(es1.logdatetime)=@iYear        
                        and day(es1.logdatetime)=@iday-1     
                        and cast(es1.logdatetime as time)>DATEADD(hour,-2,sd1.sStartTime )  
                        group by es1.EmpId,es1.LogDate),'1900-01-01 00:00:00.000') InTime            
                        ,            
                        isnull((select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate     
                        and cast(es1.logdatetime as time)<DATEADD(hour,2,sd1.sEndTime )  
                        group by es1.EmpId,es1.LogDate  
  
                        ),'1900-01-01 00:00:00.000') OutTime   
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime)sInTime,  
  
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime)as datetime) sOutTime          
                        ,'Night' ShiftType  
                        from tblEmployee e   
                        left join tblcategory c on c.iMasterId=e.iCategory            
                        left join tblDepartment d on d.iMasterid=e.iDepartment            
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation            
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        left join tblBranch br on br.iMasterid=e.iBranch    
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee   
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift  
                        where        
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear        
                        and day(logdatetime)=@iday      
                        and sa1.iShift in(  
                        select shiftID ShiftType from (  
                        select iMasterid shiftID, sStartTime,sEndTime,  
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,  
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate  
                        from tblShiftDefinition  
                        ) t where DATEDIFF(day,startdate,enddate)>0  
                        )  
                        ) t1        
                        )tfinal   
                        order by sBranch
                        end   ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                else
                {
                    strQry = $@"Alter Proc sp_DailyAttendanceDashBoard  
                        @iday int,        
                        @iMonth int,            
                        @iYear int        
                        as   
                        begin  
                        --Day Shift        
                         
                        select distinct * from (        
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)     
                        then 'Ontime' else 'Late' end loginStatus   
                        ,case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)   
                        then 'Late-In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)  
                        then 'Early Out' when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and   
                            CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)   
                            then 'Late-In and Early Out'  
                        else ''  
                        end LateINEarlyOut   
  
                        from (        
                        select e.iMasterId eMasterid,            
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,
                        isnull(br.iMasterid,0) iBranch,
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment            
                        ,isnull(desig.sName,'') sDesignation,LogDate            
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate            
                        group by es1.EmpId,es1.LogDate) InTime            
                        ,            
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate            
                        group by es1.EmpId,es1.LogDate) OutTime       
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime) sInTime,  
      
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime) as datetime) sOutTime    
                        ,'Day' ShiftType  
                        from tblEmployee e   
                        left join tblcategory c on c.iMasterId=e.iCategory            
                        left join tblDepartment d on d.iMasterid=e.iDepartment            
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation            
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        left join tblBranch br on br.iMasterid=e.iBranch    
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee   
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift  
                        where  
                        e.bEmployeeResign=0 and 
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear  
                        and day(logdatetime)=@iday      
                        and sa1.iShift in(  
                        select shiftID ShiftType from (  
                        select iMasterid shiftID, sStartTime,sEndTime,  
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,  
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate  
                        from tblShiftDefinition sd11 join tblshiftAllocation sa11 on sd11.iMasterid=sa11.iShift  
						where iday=@iday and imonth=@iMonth and iyear=@iyear and iEmployee=e.iMasterId   
                        ) t where DATEDIFF(day,startdate,enddate)<=0  
                        )  
                        ) t1        
                        )tfinal   
  
                        union all  
                        --Night shift  
  
                        select distinct * from (        
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)     
                        then 'Ontime' else 'Late' end loginStatus,   
                        case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)   
                        then 'Late-In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)  
                        then 'Early Out'   
                        when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and   
                        CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)   
                        then 'Late-In and Early Out'  
                        else ''  
                        end LateINEarlyOut  
                        from (        
                        select e.iMasterId eMasterid,            
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch, 
                        isnull(br.iMasterid,0) iBranch,
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment            
                        ,isnull(desig.sName,'') sDesignation,LogDate            
                        ,  
                        isnull((select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and   
                        MONTH(es1.logdatetime)=@iMonth and year(es1.logdatetime)=@iYear        
                        and day(es1.logdatetime)=@iday     
                        and cast(es1.logdatetime as time)>DATEADD(hour,-2,sd1.sStartTime )  
                        group by es1.EmpId,es1.LogDate),'1900-01-01 00:00:00.000') InTime            
                        ,            
                        isnull((select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where             
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate     
                        and cast(es1.logdatetime as time)<DATEADD(hour,2,sd1.sEndTime )  
                        group by es1.EmpId,es1.LogDate  
  
                        ),'1900-01-01 00:00:00.000') OutTime   
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime)sInTime,  
  
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime)as datetime) sOutTime          
                        ,'Night' ShiftType  
                        from tblEmployee e   
                        left join tblcategory c on c.iMasterId=e.iCategory            
                        left join tblDepartment d on d.iMasterid=e.iDepartment            
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation            
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode        
                        left join tblBranch br on br.iMasterid=e.iBranch    
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee   
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift  
                        where        
                        e.bEmployeeResign=0 and 
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear        
                        and day(logdatetime)=@iday      
                        and sa1.iShift in(  
                        select shiftID ShiftType from (  
                        select iMasterid shiftID, sStartTime,sEndTime,  
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,  
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate  
                        from tblShiftDefinition sd11 join tblshiftAllocation sa11 on sd11.iMasterid=sa11.iShift  
						where iday=@iday and imonth=@iMonth and iyear=@iyear and iEmployee=e.iMasterId   
                        ) t where DATEDIFF(day,startdate,enddate)>0  
                        )  
                        ) t1        
                        )tfinal where intime<>'1900-01-01 00:00:00.000'
                        order by sBranch
                        end   ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "Select * from sysObjects where name='sp_DailyAttendanceReport'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Proc sp_DailyAttendanceReport   
                    @iday int,          
                    @iMonth int,              
                    @iYear int,
                    @iEmployee int
                    as     
                    begin    
                    --Day Shift          
                           
                    select distinct * from (          
                    select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)       
                    then 'Ontime' else 'Late' end loginStatus     
                    ,case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)     
                    then 'Late In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)    
                    then 'Early Out' when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and     
                    CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)     
                    then 'Late In/Early Out'    
                    else 'Ontime'    
                    end Status     
    
                    from (          
                    select e.iMasterId eMasterid,              
                    e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,  
                    isnull(br.iMasterid,0) iBranch,  
                    isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment              
                    ,isnull(desig.sName,'') sDesignation,LogDate              
                    ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where               
                    es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate              
                    group by es1.EmpId,es1.LogDate) InTime              
                    ,              
                    (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where               
                    es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate              
                    group by es1.EmpId,es1.LogDate) OutTime         
                    ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime) sInTime,    
        
                    cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime) as datetime) sOutTime      
                    ,'Day' ShiftType    
                    from tblEmployee e     
                    left join tblcategory c on c.iMasterId=e.iCategory              
                    left join tblDepartment d on d.iMasterid=e.iDepartment              
                    left join tblDesignation desig on desig.iMasterid=e.iDesignation              
                    join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode          
                    left join tblBranch br on br.iMasterid=e.iBranch      
                    JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee     
                    join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift    
                    where e.iMasterId=@iEmployee  and       
                    MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear    
                    and day(logdatetime)=@iday        
                    and sa1.iShift in(    
                    select shiftID ShiftType from (    
                    select iMasterid shiftID, sStartTime,sEndTime,    
                    CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,    
                    dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate    
                    from tblShiftDefinition    
                    ) t where DATEDIFF(day,startdate,enddate)<=0    
                    )    
                    ) t1          
                    )tfinal     
    
                    union all    
                    --Night shift    
    
                    select distinct * from (          
                    select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)       
                    then 'Ontime' else 'Late' end loginStatus,     
                    case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)     
                    then 'Late In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)    
                    then 'Early Out'     
                    when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and     
                    CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)     
                    then 'Late In/Early Out'    
                    else 'Ontime'    
                    end Status    
                    from (          
                    select e.iMasterId eMasterid,              
                    e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,   
                    isnull(br.iMasterid,0) iBranch,  
                    isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment              
                    ,isnull(desig.sName,'') sDesignation,LogDate              
                    ,    
                    isnull((select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where               
                    es1.EmpId=e.sEmployeeCode and     
                    MONTH(es1.logdatetime)=@iMonth and year(es1.logdatetime)=@iYear          
                    and day(es1.logdatetime)=@iday-1       
                    and cast(es1.logdatetime as time)>DATEADD(hour,-2,sd1.sStartTime )    
                    group by es1.EmpId,es1.LogDate),'1900-01-01 00:00:00.000') InTime              
                    ,              
                    isnull((select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where               
                    es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate       
                    and cast(es1.logdatetime as time)<DATEADD(hour,2,sd1.sEndTime )    
                    group by es1.EmpId,es1.LogDate    
    
                    ),'1900-01-01 00:00:00.000') OutTime     
                    ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime)sInTime,    
    
                    cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime)as datetime) sOutTime            
                    ,'Night' ShiftType    
                    from tblEmployee e     
                    left join tblcategory c on c.iMasterId=e.iCategory              
                    left join tblDepartment d on d.iMasterid=e.iDepartment              
                    left join tblDesignation desig on desig.iMasterid=e.iDesignation              
                    join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode          
                    left join tblBranch br on br.iMasterid=e.iBranch      
                    JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee     
                    join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift    
                    where  e.iMasterId=@iEmployee  and         
                    MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear          
                    and day(logdatetime)=@iday        
                    and sa1.iShift in(    
                    select shiftID ShiftType from (    
                    select iMasterid shiftID, sStartTime,sEndTime,    
                    CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,    
                    dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate    
                    from tblShiftDefinition    
                    ) t where DATEDIFF(day,startdate,enddate)>0    
                    )    
                    ) t1          
                    )tfinal     
                    order by sBranch  
                    end ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                else
                {
                    strQry = $@"Alter Proc sp_DailyAttendanceReport       
                        @iday int,              
                        @iMonth int,                  
                        @iYear int,    
                        @iEmployee int    
                        as         
                        begin        
                        --Day Shift              
                               
                        select distinct * from (              
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)           
                        then 'Ontime' else 'Late' end loginStatus         
                        ,case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)         
                        then 'Late In' when  CAST(OutTime AS TIME)<CAST(sOutTime as time)        
                        then 'Early Out' when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and         
                        CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)         
                        then 'Late In/Early Out'        
                        else 'Ontime'        
                        end Status         
        
                        from (              
                        select e.iMasterId eMasterid,                  
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,      
                        isnull(br.iMasterid,0) iBranch,      
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment                  
                        ,isnull(desig.sName,'') sDesignation,LogDate                  
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where                   
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                  
                        group by es1.EmpId,es1.LogDate) InTime                  
                        ,                  
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                   
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                  
                        group by es1.EmpId,es1.LogDate) OutTime             
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime) sInTime,        
            
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime) as datetime) sOutTime          
                        ,'Day' ShiftType        
                        from tblEmployee e         
                        left join tblcategory c on c.iMasterId=e.iCategory                  
                        left join tblDepartment d on d.iMasterid=e.iDepartment                  
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation                  
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode              
                        left join tblBranch br on br.iMasterid=e.iBranch          
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee         
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift        
                        where e.iMasterId=@iEmployee  and           
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear        
                        and day(logdatetime)=@iday            
                        and sa1.iShift in(        
                        select shiftID ShiftType from (        
                        select iMasterid shiftID, sStartTime,sEndTime,        
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,        
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate        
                        from tblShiftDefinition sd11 join tblshiftAllocation sa11 on sd11.iMasterid=sa11.iShift  
                        where iday=@iday and imonth=@iMonth and iyear=@iyear and iEmployee=@iEmployee  
                        ) t where DATEDIFF(day,startdate,enddate)<=0        
                        )        
                        ) t1              
                        )tfinal         
        
                        union all        
                        --Night shift        
        
                        select distinct * from (              
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)           
                        then 'Ontime' else 'Late' end loginStatus,         
                        case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)         
                        then 'Late In' when    
                        CAST(OutTime AS TIME)<CAST(sOutTime as time)      
                        then 'Early Out'         
                        when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and         
                        CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)         
                        then 'Late In/Early Out'        
                        else 'Ontime'        
                        end Status        
                        from (              
                        select e.iMasterId eMasterid,                  
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,       
                        isnull(br.iMasterid,0) iBranch,      
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment                  
                        ,isnull(desig.sName,'') sDesignation,LogDate            
                        ,        
                        isnull((select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where                   
                        es1.EmpId=e.sEmployeeCode and         
                        MONTH(es1.logdatetime)=@iMonth and year(es1.logdatetime)=@iYear              
                        and day(es1.logdatetime)=@iday           
                        and cast(es1.logdatetime as time)>DATEADD(hour,-2,sd1.sStartTime )        
                        group by es1.EmpId,es1.LogDate),'1900-01-01 00:00:00.000') InTime                  
                        ,                  
                        isnull((select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                   
                        es1.EmpId=e.sEmployeeCode 
                        --and es1.LogDate=es.LogDate           
                        and MONTH(es1.logdatetime)=@iMonth and year(es1.logdatetime)=@iYear                
                        and day(es1.logdatetime)=@iday+1   
                        and cast(es1.logdatetime as time)<DATEADD(hour,2,sd1.sEndTime )        
                        group by es1.EmpId,es1.LogDate        
        
                        ),'1900-01-01 00:00:00.000') OutTime         
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime)sInTime,        
        
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime)as datetime) sOutTime                
                        ,'Night' ShiftType        
                        from tblEmployee e         
                        left join tblcategory c on c.iMasterId=e.iCategory                  
                        left join tblDepartment d on d.iMasterid=e.iDepartment                  
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation                  
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode              
                        left join tblBranch br on br.iMasterid=e.iBranch          
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee         
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift        
                        where  e.iMasterId=@iEmployee  and             
                        MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear              
                        and day(logdatetime)=@iday            
                        and sa1.iShift in(        
                        select shiftID ShiftType from (        
                        select iMasterid shiftID, sStartTime,sEndTime,        
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,        
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate        
                        from tblShiftDefinition sd11 join tblshiftAllocation sa11 on sd11.iMasterid=sa11.iShift  
                        where iday=@iday and imonth=@iMonth and iyear=@iyear and iEmployee=@iEmployee  
                        ) t where DATEDIFF(day,startdate,enddate)>0        
                        )        
                        ) t1              
                        )tfinal where InTime<>'1900-01-01 00:00:00.000' and OutTime<>'1900-01-01 00:00:00.000'  
                        order by sBranch      
                        end ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }


                strQry = "Select * from sysObjects where name='sp_TimeCard'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Procedure sp_TimeCard
                        @iEmployee int,      
                        @iMonth int,      
                        @iYear int  
                        as      
                        begin      
   
                           select t.*,  
                          convert(Time,OutTime-InTime) HoursWorked,'Present' status from (  
                          select distinct e.iMasterId EmployeeId,      
                          e.sEmployeeName EmployeeName,format(logDateTime,'dd-MM-yyyy') LogDate    
                          ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where       
                          es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate      
                          group by es1.EmpId,es1.LogDate) InTime      
                          ,      
                          (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where       
                          es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate      
                          group by es1.EmpId,es1.LogDate) OutTime      
                          from tblEmployee e join tblcategory c on c.iMasterId=e.iCategory      
                          join tblDepartment d on d.iMasterid=e.iDepartment      
                          join tblDesignation desig on desig.iMasterid=e.iDesignation      
                          join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode      
                          where e.iMasterid=@iEmployee
                          and MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear     
                          )t 
                          union all
                          select e.iMasterId EmployeeId,e.sEmployeeName EmployeeName,
                         format(dFromDate,'dd-MM-yyyy') LogDate,getdate() InTime,getdate() OutTime,
                         convert(time,getdate()) HoursWorked,'Leave' status
                         from tblLeaveApplication leave join tblEmployee e
                          on e.iMasterId=leave.iEmployee
                          where leave.iEmployee=@iEmployee and month(leave.dFromDate)=@iMonth
                          and year(leave.dfromDate)=@iYear
      
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "Select * from sysObjects where name='sp_leavedetail'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Procedure sp_leavedetail
                        @iEmployee int,
                        @iMonth int,      
                        @iYear int  
                        as
                        begin
                        select t.*, cast(iTotalDays-TotalLeaveTaken as float) Balance from (
                        select la.iLeaveType,la.sLeaveName,cast(ld.iTotalDays as float)iTotalDays ,
                        cast(la.ileaveDaysPermonth as float) ileaveDaysPermonth,
                        cast(isnull((select sum(fDaysTakenOnLeaveType) LeaveTakenCurrentMonth
                        from tblLeaveApplication where iEmployee=@iEmployee
                        and month(dfromDate)=@iMonth and year(dfromdate)=@iYear
                        and iLeaveType=ld.iMasterid
                        ),0)as float) LeaveTakenCurrentMonth
                        ,cast(isnull((select sum(fDaysTakenOnLeaveType) TotalLeaveTaken
                        from tblLeaveApplication where iEmployee=@iEmployee
                        and year(dfromdate)=@iYear
                        and iLeaveType=ld.iMasterid
                        ),0)as float) TotalLeaveTaken
                        from tblLeaveAllocation la 
                        join tblLeaveDefinition ld on ld.iMasterid=la.iLeaveType
                        where la.iEmployee=@iEmployee
                        )t
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "Select * from sysObjects where name='sp_getPendingLeaveDetail'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create Procedure sp_getPendingLeaveDetail  
                        @iEmployee int  
                        as   
                        begin  
                        if (@iEmployee=0)   
                        begin  
                        select la.id LeaveId, e.sEmployeeName EmployeeName,  
                        e.sEmployeeCode EmployeeCode,la.sRemarks Remarks ,  
                        repto.sEmployeeName ReportingTo,format(la.dFromDate,'dd-MM-yyyy') dDate  
                        from tblLeaveApplication la  
                        join tblEmployee e on e.iMasterId=la.iEmployee  
                        left join tblEmployee repto on e.iReportingTo=repto.iMasterId  
                        where la.iApproved1=0  
                        end  
                        else  
                        begin  
                        select la.id LeaveId, e.sEmployeeName EmployeeName,  
                        e.sEmployeeCode EmployeeCode,la.sRemarks Remarks ,  
                        repto.sEmployeeName ReportingTo,format(la.dFromDate,'dd-MM-yyyy') dDate  
                        from tblLeaveApplication la  
                        join tblEmployee e on e.iMasterId=la.iEmployee  
                        left join tblEmployee repto on e.iReportingTo=repto.iMasterId  
                        where e.iMasterId=@iEmployee and la.iApproved1=0  
                        end  
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "Select * from sysObjects where name='sp_getEmployeeLeaveStatus'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE Proc sp_getEmployeeLeaveStatus        
                        @iEmployee int ,  
                        @fromDate date,  
                        @toDate date  
                        as        
                        begin        
                        select e.sEmployeeName EmployeeName,        
                        e.sEmployeeCode EmployeeCode,format(la.dFromDate,'dd-MM-yyyy') LeaveDate,      
                        ld.sLeaveCode LeaveType,la.sRemarks Reason,  
                        isnull(approval.sEmployeeName,'') ApprovalAuthority,        
                        case when la.iApproved1=-1 then 'Rejected'         
                        when la.iApproved1=0 then 'Pending'        
                        when la.iApproved1=1 then 'Approved' end Status,      
                        la.sApprovedBy1 ApprovedRejectedBy,la.ApprovalRemarks1 AppRejReason    
                        from tblEmployee e join tblLeaveApplication la        
                        left join tblEmployee approval on approval.iMasterId=la.iApprovedAuthority1        
                        on la.iEmployee=e.iMasterId        
                        join tblLeaveDefinition ld on ld.iMasterid=la.iLeaveType        
                        where (@iEmployee=0 or e.iMasterid=@iEmployee)   
                        and dFromDate between @fromDate and @toDate  
                        order by la.dFromDate desc        
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                else
                {
                    strQry = $@"Alter Proc sp_getEmployeeLeaveStatus        
                        @iEmployee int ,  
                        @fromDate date,  
                        @toDate date  
                        as        
                        begin        
                        select la.id Leaveid,e.sEmployeeName EmployeeName,        
                        e.sEmployeeCode EmployeeCode,format(la.dFromDate,'dd-MM-yyyy') LeaveDate,      
                        ld.sLeaveCode LeaveType,la.sRemarks Reason,  
                        isnull(approval.sEmployeeName,'') ApprovalAuthority,        
                        case when la.iApproved1=-1 then 'Rejected'         
                        when la.iApproved1=0 then 'Pending'        
                        when la.iApproved1=1 then 'Approved' end Status,      
                        la.sApprovedBy1 ApprovedRejectedBy,la.ApprovalRemarks1 AppRejReason    
                        from tblEmployee e join tblLeaveApplication la        
                        left join tblEmployee approval on approval.iMasterId=la.iApprovedAuthority1        
                        on la.iEmployee=e.iMasterId        
                        join tblLeaveDefinition ld on ld.iMasterid=la.iLeaveType        
                        where (@iEmployee=0 or e.iMasterid=@iEmployee)   
                        and dFromDate between @fromDate and @toDate  
                        order by la.dFromDate desc        
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }


                strQry = "Select * from sysObjects where name='sp_getEmployeeLeaveStatusDB'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE Proc sp_getEmployeeLeaveStatusDB        
                        @iEmployee int ,  
                        @fromDate date
                        as        
                        begin        
                        select e.sEmployeeName EmployeeName,        
                        e.sEmployeeCode EmployeeCode,format(la.dFromDate,'dd-MM-yyyy') LeaveDate,      
                        ld.sLeaveCode LeaveType,la.sRemarks Reason,  
                        isnull(approval.sEmployeeName,'') ApprovalAuthority,        
                        case when la.iApproved1=-1 then 'Rejected'         
                        when la.iApproved1=0 then 'Pending'        
                        when la.iApproved1=1 then 'Approved' end Status,      
                        la.sApprovedBy1 ApprovedRejectedBy,la.ApprovalRemarks1 AppRejReason    
                        from tblEmployee e join tblLeaveApplication la        
                        left join tblEmployee approval on approval.iMasterId=la.iApprovedAuthority1        
                        on la.iEmployee=e.iMasterId        
                        join tblLeaveDefinition ld on ld.iMasterid=la.iLeaveType        
                        where (@iEmployee=0 or e.iMasterid=@iEmployee)   
                        and dFromDate>=@fromDate
                        order by la.dFromDate desc        
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                else
                {
                   
                }

                strQry = $@"Select * from sysObjects where name='sp_LastweekAttendanceReport'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE Proc sp_LastweekAttendanceReport    
                        @fromdate datetime,                
                        @todate datetime,                    
                        @iEmployee int      
                        as           
                        begin          
                        --Day Shift                
                        select format(tMain.InTime,'dd-MM-yyyy HH:mm') login,  
                        format(tMain.OutTime,'dd-MM-yyyy HH:mm') logout,Status,    
                        RIGHT('0' + CAST(DATEDIFF(MINUTE, InTime, OutTime) / 60 AS VARCHAR), 2)     
                        + ':' +     
                        RIGHT('0' + CAST(DATEDIFF(MINUTE, InTime, OutTime) % 60 AS VARCHAR), 2) AS HourWorked    
                        from (                                 
                        select distinct * from (                
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)             
                        then 'Ontime' else 'Late' end loginStatus           
                        ,case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)           
                        then 'Late In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)          
                        then 'Early Out' when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and           
                        CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)           
                        then 'Late In/Early Out'          
                        else 'Ontime'          
                        end Status           
                        from (                
                        select e.iMasterId eMasterid,                    
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,        
                        isnull(br.iMasterid,0) iBranch,        
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment                    
                        ,isnull(desig.sName,'') sDesignation,LogDate                    
                        ,(select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                        group by es1.EmpId,es1.LogDate) InTime                    
                        ,                    
                        (select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate                    
                        group by es1.EmpId,es1.LogDate) OutTime               
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime) sInTime,          
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime) as datetime) sOutTime            
                        ,'Day' ShiftType          
                        from tblEmployee e           
                        left join tblcategory c on c.iMasterId=e.iCategory                    
                        left join tblDepartment d on d.iMasterid=e.iDepartment                    
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation                    
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode                
                        left join tblBranch br on br.iMasterid=e.iBranch            
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee           
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift          
                        where e.iMasterId=@iEmployee  and             
                        --MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear and day(logdatetime)=@iday              
                        cast(logdatetime as date) between @fromdate and @todate    
                        and sa1.iShift in(          
                        select shiftID ShiftType from (          
                        select iMasterid shiftID, sStartTime,sEndTime,          
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' 
                        + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,          
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' 
                        + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate          
                        from tblShiftDefinition          
                        ) t where DATEDIFF(day,startdate,enddate)<=0          
                        )          
                        ) t1                
                        )tfinal           
                        union all          
                        --Night shift          
                        select distinct * from (                
                        select *,case when  CAST(InTime AS TIME)<=CAST(dateadd(second,59,sintime) as time)             
                        then 'Ontime' else 'Late' end loginStatus,           
                        case when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time)           
                        then 'Late In' when  CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)          
                        then 'Early Out'           
                        when CAST(InTime AS TIME)>CAST(dateadd(second,59,sintime) as time) and           
                        CAST(OutTime AS TIME)<CAST(dateadd(second,59,sOutTime) as time)           
                        then 'Late In/Early Out'          
                        else 'Ontime'          
                        end Status          
                        from (                
                        select e.iMasterId eMasterid,                    
                        e.sEmployeeName,e.sEmployeeCode ,isnull(br.sName,'') sBranch,         
                        isnull(br.iMasterid,0) iBranch,        
                        isnull(c.sName,'') sCategory,isnull(d.sName,'') sDepartment                    
                        ,isnull(desig.sName,'') sDesignation,LogDate,          
                        isnull((select min(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and           
                        --MONTH(es1.logdatetime)=@iMonth and year(es1.logdatetime)=@iYear and day(es1.logdatetime)=@iday-1             
                        cast(logdatetime as date) between @fromdate and @todate    
                        and cast(es1.logdatetime as time)>DATEADD(hour,-2,sd1.sStartTime )          
                        group by es1.EmpId,es1.LogDate),'1900-01-01 00:00:00.000') InTime                    
                        ,                    
                        isnull((select max(es1.logdatetime) from tblEmployeeTimeSheet es1 where                     
                        es1.EmpId=e.sEmployeeCode and es1.LogDate=es.LogDate             
                        and cast(es1.logdatetime as time)<DATEADD(hour,2,sd1.sEndTime )          
                        group by es1.EmpId,es1.LogDate          
                        ),'1900-01-01 00:00:00.000') OutTime           
                        ,cast(dateadd(minute,sd1.iAllowLateMinute,sd1.sStartTime) as datetime)sInTime,          
                        cast(dateadd(minute,-sd1.iAllowEarlyMinute,sd1.sEndTime)as datetime) sOutTime                  
                        ,'Night' ShiftType          
                        from tblEmployee e           
                        left join tblcategory c on c.iMasterId=e.iCategory                    
                        left join tblDepartment d on d.iMasterid=e.iDepartment                    
                        left join tblDesignation desig on desig.iMasterid=e.iDesignation                    
                        join tblEmployeeTimeSheet es on es.EmpId=e.sEmployeeCode                
                        left join tblBranch br on br.iMasterid=e.iBranch            
                        JOIN tblShiftAllocation sa1 ON e.iMasterId = sa1.iEmployee           
                        join tblShiftDefinition sd1 on sd1.iMasterid=sa1.iShift          
                        where  e.iMasterId=@iEmployee  and               
                        --MONTH(logdatetime)=@iMonth and year(logdatetime)=@iYear and day(logdatetime)=@iday              
                        cast(logdatetime as date) between @fromdate and @todate    
                        and sa1.iShift in(          
                        select shiftID ShiftType from (          
                        select iMasterid shiftID, sStartTime,sEndTime,          
                        CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' 
                        + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) startdate,          
                        dateadd(HOUR,fWorkingHour, CAST(CONVERT(varchar(10), GETDATE(), 120) + ' ' 
                        + substring(cast(sStartTime as varchar(12)),1,8) AS datetime) ) enddate          
                        from tblShiftDefinition          
                        ) t where DATEDIFF(day,startdate,enddate)>0          
                        )          
                        ) t1                
                        )tfinal           
                        )tMain    
                        order by sBranch        
                        end";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("createSPandFunctions", "createSPandFunctions:Exception: " + ex.Message);
            }
        }

        private void UpdateTable()
        {
            string strQry = string.Empty;
            string strErrMessage = string.Empty;
            DataSet ds = new DataSet();
            DataLayer dal = new DataLayer();
            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblEmployee' and c.name='bankName'";
                ds = dal.GetData(strQry, ref strErrMessage, database);

                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblEmployee ADD BankName varchar(100) Not Null DEFAULT ''
                        ,AccountNo varchar(20) Not Null DEFAULT '',
                        ifscCode varchar(10) not null DEFAULT '',BranchName varchar(100) Not Null DEFAULT ''";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblEmployee:Exception: " + ex.Message);
            }

            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblcompany' and c.name='MonthStartFrom'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblcompany
                            ADD MonthStartFrom INT NOT NULL DEFAULT 1";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblEmployee:Exception: " + ex.Message);
            }

            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblEmployee' and c.name='iBranch'";
                ds = dal.GetData(strQry, ref strErrMessage, database);

                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblEmployee
                            ADD iBranch INT NOT NULL DEFAULT 0";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblEmployee:Exception: " + ex.Message);
            }

            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblUsers' and c.name='iBranchList'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblUsers
                            ADD iBranchList varchar(200) NOT NULL DEFAULT ''";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblUsers:Exception: " + ex.Message);
            }

            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblEmployeeTimeSheet' and c.name='iLoginSMS'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblEmployeeTimeSheet
                            ADD iLoginSMS INT NOT NULL DEFAULT 0,iLogoutSMS INT NOT NULL DEFAULT 0";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblUsers:Exception: " + ex.Message);
            }

            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblBranch' and c.name='sEmailid'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblBranch
                            ADD sEmailid varchar(100) NOT NULL DEFAULT ''";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblUsers:Exception: " + ex.Message);
            }

            try
            {
                strQry = $@"select * from sys.tables t 
                    join sys.columns c on c.object_id=t.object_id
                    where t.name='tblPreference' and c.name='HrEmailId'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds != null && ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"ALTER TABLE tblPreference
                            ADD HrEmailId varchar(100) NOT NULL DEFAULT ''";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("updateTable", "tblUsers:Exception: " + ex.Message);
            }

        }

        private void CreateTables()
        {
            string strQry;
            string strErrMessage = string.Empty;
            DataSet ds;
            DataLayer dal = new();
            try
            {
                //EXECUTE SELECT QUERY FROM SQL SERVER entity framework

                strQry = $@"select * from sysObjects where name='tblCompany'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblCompany](
	                    [id] [int] IDENTITY(1,1) NOT NULL,
	                    [CompanyName] [varchar](100) NULL,
	                    [Address] [varchar](500) NULL,
	                    [City] [varchar](50) NULL,
	                    [Landmark] [varchar](100) NULL,
	                    [Logo] [varchar](max) NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblCategory'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create Table tblCategory (Sno int identity,iMasterid int primary Key,
                        sName varchar(100),sCode varchar(100),sDescription varchar(max))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }


                strQry = "select * from sysObjects where name='tblBranch'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create Table tblBranch (Sno int identity,iMasterid int primary Key,
                        sName varchar(100),sCode varchar(100),sDescription varchar(max))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblDepartment'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create Table tblDepartment (Sno int identity,iMasterid int primary Key,
                        sName varchar(100),sCode varchar(100),sDescription varchar(max))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblDesignation'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create Table tblDesignation (Sno int identity,iMasterid int Primary Key,
                        sName varchar(100),sCode varchar(100),sDescription varchar(max))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblMachineMap'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create Table tblMachineMap (Sno int identity,iMasterid int Primary Key,
                        IpAddress varchar(100),machineId varchar(50),Description varchar(max),isActive bit,
                        createdDate DateTime,updatedDate DateTime)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = $@"select c.* from sys.tables t join sys.all_columns c 
                    on c.object_id=t.object_id
                    where t.name='tblMachineMap' and c.name='machineId'
                    and c.user_type_id=167";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = "alter table tblMachineMap alter column machineId varchar(50)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblShiftDefinition'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblShiftDefinition
                        (Sno int identity,iMasterid int Primary Key,sShiftCode varchar(50),
                        sShiftName varchar(100),sStartTime time,sEndTime time
                        ,iBreakduration int,iAllowlateminute int,iAllowearlyminute int,
                        fWorkingHour float,iMinOTminute int,	
                        iMinuteToConsider int,fHalfday float,fFullDay float)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblHoliday'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblHoliday (Sno int identity,iMasterid int primary key,
                        sHolidayName varchar(100),sHolidayCode varchar(100),dDate datetime)
                    ";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblLeaveDefinition'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblLeaveDefinition(Sno int identity, iMasterid int Primary Key, 
                        sLeaveCode varchar(20), sLeaveType varchar(20), isPaid bit, iTotalDays int)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblState'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE tblState (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        StateName NVARCHAR(255),
                        CountryName NVARCHAR(255))";
                    dal.GetExecute(strQry, ref strErrMessage, database);

                    #region Insert State Data
                    strQry = $@"
                        INSERT INTO tblState (StateName, CountryName) VALUES
                        ('Andhra Pradesh', 'India'),
                        ('Arunachal Pradesh', 'India'),
                        ('Assam', 'India'),
                        ('Bihar', 'India'),
                        ('Chhattisgarh', 'India'),
                        ('Goa', 'India'),
                        ('Gujarat', 'India'),
                        ('Haryana', 'India'),
                        ('Himachal Pradesh', 'India'),
                        ('Jharkhand', 'India'),
                        ('Karnataka', 'India'),
                        ('Kerala', 'India'),
                        ('Madhya Pradesh', 'India'),
                        ('Maharashtra', 'India'),
                        ('Manipur', 'India'),
                        ('Meghalaya', 'India'),
                        ('Mizoram', 'India'),
                        ('Nagaland', 'India'),
                        ('Odisha', 'India'),
                        ('Punjab', 'India'),
                        ('Rajasthan', 'India'),
                        ('Sikkim', 'India'),
                        ('Tamil Nadu', 'India'),
                        ('Telangana', 'India'),
                        ('Tripura', 'India'),
                        ('Uttar Pradesh', 'India'),
                        ('Uttarakhand', 'India'),
                        ('West Bengal', 'India'),
                        ('Andaman and Nicobar Islands', 'India'),
                        ('Chandigarh', 'India'),
                        ('Dadra and Nagar Haveli and Daman and Diu', 'India'),
                        ('Delhi', 'India'),
                        ('Jammu and Kashmir', 'India'),
                        ('Ladakh', 'India'),
                        ('Lakshadweep', 'India'),
                        ('Puducherry', 'India');";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                    #endregion
                }

                strQry = "select * from sysObjects where name='tblCountries'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE tblCountries (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        CountryName NVARCHAR(255),
                        CountryCode NVARCHAR(10))";
                    dal.GetExecute(strQry, ref strErrMessage, database);

                    #region Insert Country Data
                    strQry = $@"INSERT INTO tblCountries (CountryName, CountryCode) VALUES
                        ('India', 'IN'),
                        ('United States', 'US'),
                        ('United Kingdom', 'GB'),
                        ('Canada', 'CA'),
                        ('Australia', 'AU'),
                        ('Germany', 'DE'),
                        ('France', 'FR'),
                        ('Japan', 'JP'),
                        ('China', 'CN'),
                        ('Brazil', 'BR')";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                    #endregion
                }

                strQry = "select * from sysObjects where name='tblCities'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE tblCities (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        CityName NVARCHAR(255),
                        StateName NVARCHAR(255),
                        CountryName NVARCHAR(255))";
                    dal.GetExecute(strQry, ref strErrMessage, database);

                    #region Insert Country Data
                    strQry = $@"INSERT INTO tblCities (CityName, StateName, CountryName) VALUES
                        ('Mumbai', 'Maharashtra', 'India'),
                        ('Delhi', 'Delhi', 'India'),
                        ('Bengaluru', 'Karnataka', 'India'),
                        ('Hyderabad', 'Telangana', 'India'),
                        ('Chennai', 'Tamil Nadu', 'India'),
                        ('Kolkata', 'West Bengal', 'India'),
                        ('Pune', 'Maharashtra', 'India'),
                        ('Ahmedabad', 'Gujarat', 'India'),
                        ('Jaipur', 'Rajasthan', 'India'),
                        ('Lucknow', 'Uttar Pradesh', 'India'),
                        ('Kanpur', 'Uttar Pradesh', 'India'),
                        ('Nagpur', 'Maharashtra', 'India'),
                        ('Indore', 'Madhya Pradesh', 'India'),
                        ('Bhopal', 'Madhya Pradesh', 'India'),
                        ('Coimbatore', 'Tamil Nadu', 'India'),
                        ('Thane', 'Maharashtra', 'India'),
                        ('Patna', 'Bihar', 'India'),
                        ('Vadodara', 'Gujarat', 'India'),
                        ('Ghaziabad', 'Uttar Pradesh', 'India'),
                        ('Ludhiana', 'Punjab', 'India')";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                    #endregion
                }

                strQry = "select * from sysObjects where name='tblShiftwiseWeekend'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblShiftwiseWeekend](
	                    [SNo] [int] IDENTITY(1,1) NOT NULL,
	                    [iShiftid] [int] NULL,
	                    [sday] [varchar](10) NULL,
	                    [week1Selected] [bit] NULL,
	                    [week1WeekendType] [varchar](10) NULL,
	                    [week2Selected] [bit] NULL,
	                    [week2WeekendType] [varchar](10) NULL,
	                    [week3Selected] [bit] NULL,
	                    [week3WeekendType] [varchar](10) NULL,
	                    [week4Selected] [bit] NULL,
	                    [week4WeekendType] [varchar](10) NULL,
	                    [week5Selected] [bit] NULL,
	                    [week5WeekendType] [varchar](10) NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblShiftAllocation'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblShiftAllocation(Sno int identity,iCategory int,
                        iEmployee int,iShift int,shiftDate datetime)";
                }

                strQry = "select * from sysObjects where name='tblEmployee'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblEmployee](
	                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [iMasterId] [int] NOT NULL primary key,
	                    [sEmployeeName] [varchar](100) NULL,
	                    [sEmployeeCode] [varchar](50) NULL,
	                    [dHireDate] [datetime] NULL,
	                    [sGender] [varchar](10) NULL,
	                    [iCategory] [int] NULL,
	                    [iDesignation] [int] NULL,
	                    [iDepartment] [int] NULL,
	                    [iShift] [int] NULL,
	                    [iReportingTo] [int] null,
	                    [sPanNo] [varchar](10) NULL,
	                    [sAadharNo] [varchar](12) NULL,
	                    [sPassportNo] [varchar](20) NULL,
	                    [bOTStatus] [bit] NULL,
	                    [bEmployeeResign] [bit] NULL,
	                    [bAutoShift] [bit] NULL,
                        [bPermanent] [bit] NULL,
	                    [DOB] [datetime] NULL,
	                    [iCountry] [int] NULL,
	                    [iState] [int] NULL,
	                    [iCity] [int] NULL,
	                    [sPhoneNo] [varchar](15) NULL,
	                    [sEmailId] [varchar](50) NULL,
	                    [sAddress1] [varchar](100) NULL,
	                    [sAddress2] [varchar](100) NULL,
	                    [sLandmark] [varchar](100) NULL,
	                    [sPincode] [varchar](10) NULL,
	                    [sMaritalStatus] [varchar](20) NULL,
	                    [sEmergencyContact] [varchar](20) NULL,
	                    [sImagePath] [varchar](100) NULL,
	                    [sImage] [varchar](max) NULL,
	                    [dCreatedDate] [datetime] NULL,
	                    [dModifiedDate] [datetime] NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblEmployeePermanent'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblEmployeePermanent(Sno int identity, imasterid int,
                        startDate datetime,status varchar(1))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblEmployeeTimeSheet'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE tblEmployeeTimeSheet (sno int identity,IPAddress varchar(100),
                        EmpId varchar(50),EmpName varchar(50),
                        LogDate varchar(50),LogTime varchar(50),logDateTime datetime)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblshiftAllocation'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblshiftAllocation(Sno int identity, AllocationType varchar(15),
                        iCategory int, iEmployee int, TimeFrame varchar(10), imonth int, iyear int, iday int,
                        dDate datetime, iShift int
                        , constraint ATCheck check(allocationType in ('Employeewise', 'categorywise'))
                        , constraint TFCheck check(TimeFrame in ('Monthly', 'Yearly')))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='UserActivityLog'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table UserActivityLog(Id int identity,UserId varchar(10)
                        ,UserName varchar(50),Action varchar(max),Controller varchar(max)
                        ,Description varchar(max),Timestamp datetime,IPAddress varchar(20))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblUsers'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblUsers](
	                    [id] [int] IDENTITY(1,1) NOT NULL,[UserName] [varchar](50) NULL,
	                    [Email] [varchar](50) NULL,PasswordHash NVARCHAR(256),Salt NVARCHAR(100),
	                    [iEmployee] [int] NULL,[Role] [varchar](20) NULL,sImage varchar(max))";
                    dal.GetExecute(strQry, ref strErrMessage, database);

                    //setting default admin user password

                    strQry = $@"Insert into tblUsers values('admin','',N'X/C+iE4CCdGFGZ1jOsgPtp9xeZNOA5xztWMvdpybbfY=',
                        N'mdjhAc4vQz0C+3Q3zfyfXw==',0,'Admin',null)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblUserRights'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblUserRights (id int identity,Role varchar(50),Menuitem varchar(100))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblLeaveApplication'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblLeaveApplication](
	                    [id] [int] IDENTITY(1,1) NOT NULL,
	                    [iEmployee] [int] NULL,
	                    [dFromDate] [datetime] NULL,
	                    [dToDate] [datetime] NULL,
	                    [fTotalDaysTaken] [float] NULL,
	                    [iLeaveType] [int] NULL,
	                    [fDaysTakenOnLeaveType] [float] NULL,
	                    [sRemarks] [varchar](max) NULL,
	                    [isfullday] [bit] NULL,
	                    [iApproved1] [int] NULL,
	                    [iApproved2] [int] NULL,
	                    [iApproved3] [int] NULL,
	                    [sApprovedBy1] [varchar](50) NULL,
	                    [sApprovedBy2] [varchar](50) NULL,
	                    [sApprovedBy3] [varchar](50) NULL,
	                    [iApprovedAuthority1] [int] NULL,
	                    [iApprovedAuthority2] [int] NULL,
	                    [iApprovedAuthority3] [int] NULL,
	                    [LeaveApplicationId1] [varchar](max) NULL,
	                    [LeaveApplicationId2] [varchar](max) NULL,
	                    [LeaveApplicationId3] [varchar](max) NULL,
	                    [ApprovalRemarks1] varchar(100),
	                    [ApprovalRemarks2] varchar(100),
	                    [ApprovalRemarks3] varchar(100),
	                    [LeaveAppliedTimeStamp] [datetime] NULL,
	                    [LeaveApprovedTimestamp] [datetime] NULL
                    )";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblLeaveAllocation'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblLeaveAllocation](
	                    [Id] [int] IDENTITY(1,1) NOT NULL,[iEmployee] [int] NULL,
	                    [iLeaveType] [int] NULL,[sLeaveName] [varchar](100) NULL,
	                    [iLeaveDaysPerMonth] [int] NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblMailConfiguration'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"Create table tblMailConfiguration(id int identity,EmailType varchar(20),
                        SmtpHost varchar(50),
                        SmtpPort int,SmtpUsername varchar(50),SmtpPassword varchar(100),
                        SmtpSsl bit,outlookEmail varchar(100),outlookPassword varchar(100))";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblPreference'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblPreference](
	                    [id] [int] IDENTITY(1,1) NOT NULL,
	                    [secLvlLeaveApproval] [bit] NULL,
	                    [secLvlLeaveAppUser] [varchar](50) NULL,
	                    [secLvlLeaveAppUserMail] [varchar](100) NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = "select * from sysObjects where name='tblEarningDeductionMaster'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblEarningDeductionMaster](
	                    [id] [int] IDENTITY(1,1) NOT NULL,[iType] [int] NULL,
	                    [TypeName] [varchar](100) NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
                strQry = "select * from sysObjects where name='tblEmployeeSalaryDefinition'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblEmployeeSalaryDefinition](
	                    [id] [int] IDENTITY(1,1) NOT NULL,[iEmployeeId] [int] NULL,
	                    [iEarningDeductionType] [int] NULL,[EarningDeductionTypeName] [varchar](100) NULL,
	                    [Amount] [float] NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = $@"select * from sysObjects where name='tblExpenses'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"CREATE TABLE [dbo].[tblExpenses](
	                    [id] [int] IDENTITY(1,1) NOT NULL,[iEmployee] [int] NULL,[Description] [varchar](max) NULL,
	                    [Remarks] [varchar](max) NULL,[Amount] [float] NULL,[sImage] [varchar](max),
	                    [ApprovedAmount] [float] NULL,[ApprovalStatus] [int] NULL,[ApprovedBy] [int] NULL,
	                    [ExpenseDate] [datetime] NULL,[CreatedDate] [datetime] NULL,[ModifiedDate] [datetime] NULL,
	                    [ApprovedDate] [datetime] NULL)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }

                strQry = $@"Select * from sysObjects where name='tblAbsentEmployeeSMS'";
                ds = dal.GetData(strQry, ref strErrMessage, database);
                if (ds == null || ds.Tables[0].Rows.Count <= 0)
                {
                    strQry = $@"create table tblAbsentEmployeeSMS(Empid varchar(50),EmpName varchar(200),dDate date)";
                    dal.GetExecute(strQry, ref strErrMessage, database);
                }
            }
            catch (Exception ex)
            {
                dal.WriteLog("createSPandFunctions", "createTable:Exception: " + ex.Message);
            }
        }
    }
}
