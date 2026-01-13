import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatCard } from "@/components/ui/stat-card";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
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
  CreditCard,
  TrendingUp,
  Users,
  IndianRupee,
  FileSpreadsheet,
  FileText,
  Building2,
} from "lucide-react";

const departmentCostData = [
  { name: "IT", value: 4500000, color: "hsl(var(--primary))" },
  { name: "Sales", value: 3200000, color: "hsl(var(--success))" },
  { name: "Finance", value: 1800000, color: "hsl(var(--warning))" },
  { name: "HR", value: 1200000, color: "hsl(var(--info))" },
  { name: "Marketing", value: 1500000, color: "hsl(var(--chart-5))" },
];

const monthlyPayrollData = [
  { month: "Aug", payroll: 11500000 },
  { month: "Sep", payroll: 11800000 },
  { month: "Oct", payroll: 12000000 },
  { month: "Nov", payroll: 12200000 },
  { month: "Dec", payroll: 12500000 },
  { month: "Jan", payroll: 12200000 },
];

const payrollRecords = [
  { id: "EMP001", name: "Sarah Johnson", dept: "IT", gross: 95000, deductions: 19000, net: 76000, status: "Paid" },
  { id: "EMP002", name: "Mike Davis", dept: "Sales", gross: 85000, deductions: 17000, net: 68000, status: "Paid" },
  { id: "EMP003", name: "Emily Brown", dept: "HR", gross: 75000, deductions: 15000, net: 60000, status: "Paid" },
  { id: "EMP004", name: "John Smith", dept: "Finance", gross: 90000, deductions: 18000, net: 72000, status: "Pending" },
  { id: "EMP005", name: "Lisa Wilson", dept: "Marketing", gross: 70000, deductions: 14000, net: 56000, status: "Paid" },
];

export default function PayrollReport() {
  const [month, setMonth] = useState("jan-2026");
  const [department, setDepartment] = useState("all");

  const handleExportExcel = () => {
    toast.success("Exporting to Excel...");
  };

  const handleExportPDF = () => {
    toast.success("Exporting to PDF...");
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("en-IN", {
      style: "currency",
      currency: "INR",
      maximumFractionDigits: 0,
    }).format(value);
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Payroll Report"
          subtitle="Salary processing and analysis"
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
        <StatCard title="Total Payroll" value="₹1.22 Cr" icon={IndianRupee} variant="default" />
        <StatCard title="Total Deductions" value="₹24.5 L" icon={TrendingUp} variant="danger" />
        <StatCard title="Employees" value="50" icon={Users} variant="info" />
        <StatCard title="Avg CTC" value="₹12 L" icon={CreditCard} variant="success" />
      </div>

      {/* Filters */}
      <Card className="hrms-card">
        <CardContent className="p-4">
          <div className="flex flex-wrap gap-4">
            <Select value={month} onValueChange={setMonth}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Month" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="jan-2026">January 2026</SelectItem>
                <SelectItem value="dec-2025">December 2025</SelectItem>
                <SelectItem value="nov-2025">November 2025</SelectItem>
                <SelectItem value="oct-2025">October 2025</SelectItem>
              </SelectContent>
            </Select>
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
          </div>
        </CardContent>
      </Card>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Building2 className="h-5 w-5 text-primary" />
              Department-wise Payroll Cost
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={departmentCostData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={2}
                    dataKey="value"
                    label={({ name }) => name}
                  >
                    {departmentCostData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value: number) => formatCurrency(value)} />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary" />
              Monthly Payroll Trend
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={monthlyPayrollData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis dataKey="month" className="text-xs" />
                  <YAxis tickFormatter={(value) => `₹${(value / 1000000).toFixed(1)}M`} className="text-xs" />
                  <Tooltip formatter={(value: number) => formatCurrency(value)} />
                  <Line
                    type="monotone"
                    dataKey="payroll"
                    name="Payroll"
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

      {/* Payroll Table */}
      <Card className="hrms-card">
        <CardHeader className="pb-2">
          <CardTitle className="text-lg font-display">Payroll Summary</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Employee ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Department</TableHead>
                  <TableHead className="text-right">Gross</TableHead>
                  <TableHead className="text-right">Deductions</TableHead>
                  <TableHead className="text-right">Net Pay</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {payrollRecords.map((record) => (
                  <TableRow key={record.id}>
                    <TableCell className="font-medium">{record.id}</TableCell>
                    <TableCell>{record.name}</TableCell>
                    <TableCell>{record.dept}</TableCell>
                    <TableCell className="text-right">₹{record.gross.toLocaleString()}</TableCell>
                    <TableCell className="text-right text-destructive">-₹{record.deductions.toLocaleString()}</TableCell>
                    <TableCell className="text-right font-semibold text-primary">₹{record.net.toLocaleString()}</TableCell>
                    <TableCell>
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                        record.status === "Paid" ? "bg-success/10 text-success" : "bg-warning/10 text-warning"
                      }`}>
                        {record.status}
                      </span>
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
