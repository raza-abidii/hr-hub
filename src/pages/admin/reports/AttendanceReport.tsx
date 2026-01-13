import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatCard } from "@/components/ui/stat-card";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { toast } from "sonner";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
} from "recharts";
import {
  Users,
  UserX,
  Clock,
  AlertTriangle,
  Download,
  FileSpreadsheet,
  FileText,
  Filter,
  Calendar,
  TrendingUp,
} from "lucide-react";

const dailyAttendanceData = [
  { date: "Jan 1", present: 42, absent: 3, leave: 5 },
  { date: "Jan 2", present: 45, absent: 2, leave: 3 },
  { date: "Jan 3", present: 44, absent: 4, leave: 2 },
  { date: "Jan 4", present: 40, absent: 5, leave: 5 },
  { date: "Jan 5", present: 0, absent: 0, leave: 0 },
  { date: "Jan 6", present: 43, absent: 3, leave: 4 },
  { date: "Jan 7", present: 46, absent: 1, leave: 3 },
];

const departmentData = [
  { name: "IT", value: 95, color: "hsl(var(--primary))" },
  { name: "HR", value: 92, color: "hsl(var(--success))" },
  { name: "Sales", value: 88, color: "hsl(var(--warning))" },
  { name: "Finance", value: 96, color: "hsl(var(--info))" },
  { name: "Marketing", value: 90, color: "hsl(var(--chart-5))" },
];

const monthlyTrendData = [
  { month: "Aug", attendance: 91 },
  { month: "Sep", attendance: 93 },
  { month: "Oct", attendance: 89 },
  { month: "Nov", attendance: 94 },
  { month: "Dec", attendance: 92 },
  { month: "Jan", attendance: 95 },
];

const attendanceRecords = [
  { id: "EMP001", name: "Sarah Johnson", dept: "IT", date: "Jan 7, 2026", clockIn: "09:00 AM", clockOut: "06:15 PM", hours: "9h 15m", status: "present" as const },
  { id: "EMP002", name: "Mike Davis", dept: "Sales", date: "Jan 7, 2026", clockIn: "09:15 AM", clockOut: "06:00 PM", hours: "8h 45m", status: "present" as const, isLate: true },
  { id: "EMP003", name: "Emily Brown", dept: "HR", date: "Jan 7, 2026", clockIn: "-", clockOut: "-", hours: "-", status: "leave" as const },
  { id: "EMP004", name: "John Smith", dept: "Finance", date: "Jan 7, 2026", clockIn: "08:55 AM", clockOut: "05:45 PM", hours: "8h 50m", status: "present" as const },
  { id: "EMP005", name: "Lisa Wilson", dept: "Marketing", date: "Jan 7, 2026", clockIn: "-", clockOut: "-", hours: "-", status: "absent" as const },
  { id: "EMP006", name: "David Lee", dept: "IT", date: "Jan 7, 2026", clockIn: "09:02 AM", clockOut: "-", hours: "Working", status: "present" as const },
];

export default function AttendanceReport() {
  const [dateRange, setDateRange] = useState("today");
  const [department, setDepartment] = useState("all");
  const [status, setStatus] = useState("all");

  const handleExportExcel = () => {
    toast.success("Exporting to Excel...");
    // In real implementation, this would generate and download an Excel file
  };

  const handleExportPDF = () => {
    toast.success("Exporting to PDF...");
    // In real implementation, this would generate and download a PDF file
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Attendance Report"
          subtitle="Comprehensive attendance analytics and reporting"
        />
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleExportExcel}>
            <FileSpreadsheet className="h-4 w-4 mr-2" />
            Excel
          </Button>
          <Button variant="outline" onClick={handleExportPDF}>
            <FileText className="h-4 w-4 mr-2" />
            PDF
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="hrms-card">
        <CardContent className="p-4">
          <div className="flex flex-wrap gap-4">
            <div className="flex items-center gap-2">
              <Calendar className="h-4 w-4 text-muted-foreground" />
              <Select value={dateRange} onValueChange={setDateRange}>
                <SelectTrigger className="w-40">
                  <SelectValue placeholder="Date Range" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="today">Today</SelectItem>
                  <SelectItem value="week">This Week</SelectItem>
                  <SelectItem value="month">This Month</SelectItem>
                  <SelectItem value="quarter">This Quarter</SelectItem>
                  <SelectItem value="custom">Custom Range</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <Select value={department} onValueChange={setDepartment}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Department" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Departments</SelectItem>
                <SelectItem value="it">IT</SelectItem>
                <SelectItem value="hr">HR</SelectItem>
                <SelectItem value="sales">Sales</SelectItem>
                <SelectItem value="finance">Finance</SelectItem>
                <SelectItem value="marketing">Marketing</SelectItem>
              </SelectContent>
            </Select>
            <Select value={status} onValueChange={setStatus}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="present">Present</SelectItem>
                <SelectItem value="absent">Absent</SelectItem>
                <SelectItem value="leave">On Leave</SelectItem>
                <SelectItem value="late">Late</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Present Today" value="46" icon={Users} variant="success" />
        <StatCard title="Absent" value="4" icon={UserX} variant="danger" />
        <StatCard title="On Leave" value="3" icon={Calendar} variant="info" />
        <StatCard title="Late Arrivals" value="2" icon={AlertTriangle} variant="warning" />
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Daily Attendance Chart */}
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary" />
              Daily Attendance (This Week)
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={dailyAttendanceData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis dataKey="date" className="text-xs" />
                  <YAxis className="text-xs" />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "hsl(var(--card))",
                      border: "1px solid hsl(var(--border))",
                      borderRadius: "8px",
                    }}
                  />
                  <Legend />
                  <Bar dataKey="present" name="Present" fill="hsl(var(--success))" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="absent" name="Absent" fill="hsl(var(--destructive))" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="leave" name="Leave" fill="hsl(var(--info))" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        {/* Department-wise Attendance */}
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Users className="h-5 w-5 text-primary" />
              Department-wise Attendance %
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={departmentData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={2}
                    dataKey="value"
                    label={({ name, value }) => `${name}: ${value}%`}
                  >
                    {departmentData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "hsl(var(--card))",
                      border: "1px solid hsl(var(--border))",
                      borderRadius: "8px",
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        {/* Monthly Trend */}
        <Card className="hrms-card lg:col-span-2">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Clock className="h-5 w-5 text-primary" />
              Monthly Attendance Trend
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={monthlyTrendData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis dataKey="month" className="text-xs" />
                  <YAxis domain={[80, 100]} className="text-xs" />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "hsl(var(--card))",
                      border: "1px solid hsl(var(--border))",
                      borderRadius: "8px",
                    }}
                  />
                  <Line
                    type="monotone"
                    dataKey="attendance"
                    name="Attendance %"
                    stroke="hsl(var(--primary))"
                    strokeWidth={3}
                    dot={{ fill: "hsl(var(--primary))", strokeWidth: 2 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Attendance Table */}
      <Card className="hrms-card">
        <CardHeader className="pb-2">
          <CardTitle className="text-lg font-display">Detailed Attendance Records</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Employee ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Department</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Clock In</TableHead>
                  <TableHead>Clock Out</TableHead>
                  <TableHead>Hours</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {attendanceRecords.map((record) => (
                  <TableRow key={`${record.id}-${record.date}`}>
                    <TableCell className="font-medium">{record.id}</TableCell>
                    <TableCell>{record.name}</TableCell>
                    <TableCell>{record.dept}</TableCell>
                    <TableCell>{record.date}</TableCell>
                    <TableCell>{record.clockIn}</TableCell>
                    <TableCell>{record.clockOut}</TableCell>
                    <TableCell className="font-semibold text-primary">{record.hours}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        {record.isLate && <StatusBadge variant="late" />}
                        <StatusBadge variant={record.status} />
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
