import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "@/lib/auth-context";
import { AppLayout } from "@/components/layout/AppLayout";
import { EmployeeLayout } from "@/components/layout/EmployeeLayout";

// Pages
import Index from "./pages/Index";
import Login from "./pages/Login";
import NotFound from "./pages/NotFound";

// Admin Pages
import AdminDashboard from "./pages/admin/Dashboard";
import AddEmployee from "./pages/admin/AddEmployee";
import LeaveApproval from "./pages/admin/LeaveApproval";
import UserManagement from "./pages/admin/UserManagement";
import EmployeeMaster from "./pages/admin/masters/EmployeeMaster";
import DepartmentMaster from "./pages/admin/masters/DepartmentMaster";
import DesignationMaster from "./pages/admin/masters/DesignationMaster";
import HolidayMaster from "./pages/admin/masters/HolidayMaster";
import ShiftMaster from "./pages/admin/masters/ShiftMaster";
import LeaveTypeMaster from "./pages/admin/masters/LeaveTypeMaster";
import CategoryMaster from "./pages/admin/masters/CategoryMaster";
import CompanyMaster from "./pages/admin/masters/CompanyMaster";
import MachineMaster from "./pages/admin/masters/MachineMaster";
import PayrollMaster from "./pages/admin/masters/PayrollMaster";

// Admin Reports
import AttendanceReport from "./pages/admin/reports/AttendanceReport";
import LeaveReport from "./pages/admin/reports/LeaveReport";
import PayrollReport from "./pages/admin/reports/PayrollReport";
import EmployeeReport from "./pages/admin/reports/EmployeeReport";

// Employee Pages
import EmployeeDashboard from "./pages/employee/Dashboard";
import MyAttendance from "./pages/employee/MyAttendance";
import LeaveManagement from "./pages/employee/LeaveManagement";
import Payroll from "./pages/employee/Payroll";
import EmployeeDirectory from "./pages/employee/EmployeeDirectory";

// Shared Pages
import Notifications from "./pages/shared/Notifications";
import Profile from "./pages/shared/Profile";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <AuthProvider>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Index />} />
            <Route path="/login" element={<Login />} />

            {/* Admin Routes */}
            <Route path="/admin" element={<AppLayout />}>
              <Route index element={<AdminDashboard />} />
              <Route path="add-employee" element={<AddEmployee />} />
              <Route path="leave-approval" element={<LeaveApproval />} />
              <Route path="users" element={<UserManagement />} />
              <Route path="masters/employee" element={<EmployeeMaster />} />
              <Route path="masters/department" element={<DepartmentMaster />} />
              <Route path="masters/designation" element={<DesignationMaster />} />
              <Route path="masters/holiday" element={<HolidayMaster />} />
              <Route path="masters/shift" element={<ShiftMaster />} />
              <Route path="masters/leave-type" element={<LeaveTypeMaster />} />
              <Route path="masters/category" element={<CategoryMaster />} />
              <Route path="masters/company" element={<CompanyMaster />} />
              <Route path="masters/machine" element={<MachineMaster />} />
              <Route path="masters/payroll" element={<PayrollMaster />} />
              <Route path="reports/attendance" element={<AttendanceReport />} />
              <Route path="reports/leave" element={<LeaveReport />} />
              <Route path="reports/payroll" element={<PayrollReport />} />
              <Route path="reports/employee" element={<EmployeeReport />} />
              <Route path="notifications" element={<Notifications />} />
              <Route path="profile" element={<Profile />} />
            </Route>

            {/* Employee Routes - with bottom navigation */}
            <Route path="/employee" element={<EmployeeLayout />}>
              <Route index element={<EmployeeDashboard />} />
              <Route path="attendance" element={<MyAttendance />} />
              <Route path="leave" element={<LeaveManagement />} />
              <Route path="payroll" element={<Payroll />} />
              <Route path="directory" element={<EmployeeDirectory />} />
              <Route path="notifications" element={<Notifications />} />
              <Route path="profile" element={<Profile />} />
            </Route>

            {/* Catch all */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
    </AuthProvider>
  </QueryClientProvider>
);

export default App;
