import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatCard } from "@/components/ui/stat-card";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
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
} from "recharts";
import {
  Calendar,
  Clock,
  CheckCircle2,
  XCircle,
  FileSpreadsheet,
  FileText,
  TrendingUp,
} from "lucide-react";

const leaveTypeData = [
  { name: "Casual Leave", used: 45, color: "hsl(var(--primary))" },
  { name: "Sick Leave", used: 28, color: "hsl(var(--warning))" },
  { name: "Earned Leave", used: 62, color: "hsl(var(--success))" },
  { name: "Compensatory", used: 12, color: "hsl(var(--info))" },
];

const departmentLeaveData = [
  { dept: "IT", casual: 12, sick: 8, earned: 15 },
  { dept: "HR", casual: 5, sick: 3, earned: 8 },
  { dept: "Sales", casual: 15, sick: 10, earned: 20 },
  { dept: "Finance", casual: 8, sick: 4, earned: 12 },
  { dept: "Marketing", casual: 5, sick: 3, earned: 7 },
];

const leaveBalanceRecords = [
  { id: "EMP001", name: "Sarah Johnson", dept: "IT", cl: { used: 2, total: 12 }, sl: { used: 1, total: 10 }, el: { used: 5, total: 18 } },
  { id: "EMP002", name: "Mike Davis", dept: "Sales", cl: { used: 4, total: 12 }, sl: { used: 2, total: 10 }, el: { used: 8, total: 18 } },
  { id: "EMP003", name: "Emily Brown", dept: "HR", cl: { used: 3, total: 12 }, sl: { used: 0, total: 10 }, el: { used: 3, total: 18 } },
  { id: "EMP004", name: "John Smith", dept: "Finance", cl: { used: 1, total: 12 }, sl: { used: 3, total: 10 }, el: { used: 6, total: 18 } },
];

export default function LeaveReport() {
  const [department, setDepartment] = useState("all");
  const [leaveType, setLeaveType] = useState("all");

  const handleExportExcel = () => {
    toast.success("Exporting to Excel...");
  };

  const handleExportPDF = () => {
    toast.success("Exporting to PDF...");
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Leave Report"
          subtitle="Leave utilization and balance tracking"
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

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Total Requests" value="147" icon={Calendar} variant="default" />
        <StatCard title="Approved" value="128" icon={CheckCircle2} variant="success" />
        <StatCard title="Pending" value="12" icon={Clock} variant="warning" />
        <StatCard title="Rejected" value="7" icon={XCircle} variant="danger" />
      </div>

      {/* Filters */}
      <Card className="hrms-card">
        <CardContent className="p-4">
          <div className="flex flex-wrap gap-4">
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
              </SelectContent>
            </Select>
            <Select value={leaveType} onValueChange={setLeaveType}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Leave Type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                <SelectItem value="casual">Casual Leave</SelectItem>
                <SelectItem value="sick">Sick Leave</SelectItem>
                <SelectItem value="earned">Earned Leave</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary" />
              Leave Type Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={leaveTypeData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={2}
                    dataKey="used"
                    label={({ name, used }) => `${name}: ${used}`}
                  >
                    {leaveTypeData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Calendar className="h-5 w-5 text-primary" />
              Department-wise Leave Usage
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={departmentLeaveData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis dataKey="dept" className="text-xs" />
                  <YAxis className="text-xs" />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="casual" name="Casual" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="sick" name="Sick" fill="hsl(var(--warning))" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="earned" name="Earned" fill="hsl(var(--success))" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Leave Balance Table */}
      <Card className="hrms-card">
        <CardHeader className="pb-2">
          <CardTitle className="text-lg font-display">Leave Balance Summary</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Employee ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Department</TableHead>
                  <TableHead>Casual Leave</TableHead>
                  <TableHead>Sick Leave</TableHead>
                  <TableHead>Earned Leave</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {leaveBalanceRecords.map((record) => (
                  <TableRow key={record.id}>
                    <TableCell className="font-medium">{record.id}</TableCell>
                    <TableCell>{record.name}</TableCell>
                    <TableCell>{record.dept}</TableCell>
                    <TableCell>
                      <span className="text-primary font-semibold">{record.cl.total - record.cl.used}</span>
                      <span className="text-muted-foreground">/{record.cl.total}</span>
                    </TableCell>
                    <TableCell>
                      <span className="text-primary font-semibold">{record.sl.total - record.sl.used}</span>
                      <span className="text-muted-foreground">/{record.sl.total}</span>
                    </TableCell>
                    <TableCell>
                      <span className="text-primary font-semibold">{record.el.total - record.el.used}</span>
                      <span className="text-muted-foreground">/{record.el.total}</span>
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
