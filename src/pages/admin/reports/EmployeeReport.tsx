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
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
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
  Users,
  UserPlus,
  UserCheck,
  Briefcase,
  FileSpreadsheet,
  FileText,
  Building2,
} from "lucide-react";

const genderData = [
  { name: "Male", value: 32, color: "hsl(var(--primary))" },
  { name: "Female", value: 18, color: "hsl(var(--success))" },
];

const departmentStrengthData = [
  { dept: "IT", count: 15 },
  { dept: "Sales", count: 12 },
  { dept: "HR", count: 5 },
  { dept: "Finance", count: 8 },
  { dept: "Marketing", count: 10 },
];

const tenureData = [
  { range: "<1 yr", count: 8 },
  { range: "1-3 yr", count: 18 },
  { range: "3-5 yr", count: 12 },
  { range: "5-10 yr", count: 8 },
  { range: ">10 yr", count: 4 },
];

const employees = [
  { id: "EMP001", name: "Sarah Johnson", email: "sarah@sfon.com", dept: "IT", designation: "Senior Developer", joining: "Jan 15, 2022", type: "Full-time", status: "Active" },
  { id: "EMP002", name: "Mike Davis", email: "mike@sfon.com", dept: "Sales", designation: "Sales Manager", joining: "Mar 20, 2021", type: "Full-time", status: "Active" },
  { id: "EMP003", name: "Emily Brown", email: "emily@sfon.com", dept: "HR", designation: "HR Executive", joining: "Jun 10, 2023", type: "Full-time", status: "Active" },
  { id: "EMP004", name: "John Smith", email: "john@sfon.com", dept: "Finance", designation: "Financial Analyst", joining: "Sep 5, 2020", type: "Full-time", status: "Active" },
  { id: "EMP005", name: "Lisa Wilson", email: "lisa@sfon.com", dept: "Marketing", designation: "Marketing Lead", joining: "Nov 12, 2022", type: "Part-time", status: "Active" },
];

export default function EmployeeReport() {
  const [department, setDepartment] = useState("all");
  const [employmentType, setEmploymentType] = useState("all");

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
          title="Employee Report"
          subtitle="Comprehensive employee data analysis"
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
        <StatCard title="Total Employees" value="50" icon={Users} variant="default" />
        <StatCard title="New This Month" value="3" icon={UserPlus} variant="success" />
        <StatCard title="In Probation" value="5" icon={UserCheck} variant="warning" />
        <StatCard title="Departments" value="5" icon={Building2} variant="info" />
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
            <Select value={employmentType} onValueChange={setEmploymentType}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                <SelectItem value="full-time">Full-time</SelectItem>
                <SelectItem value="part-time">Part-time</SelectItem>
                <SelectItem value="contract">Contract</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Users className="h-5 w-5 text-primary" />
              Gender Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-56">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={genderData}
                    cx="50%"
                    cy="50%"
                    innerRadius={50}
                    outerRadius={80}
                    paddingAngle={2}
                    dataKey="value"
                    label={({ name, value }) => `${name}: ${value}`}
                  >
                    {genderData.map((entry, index) => (
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
              <Building2 className="h-5 w-5 text-primary" />
              Department Strength
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-56">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={departmentStrengthData} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis type="number" className="text-xs" />
                  <YAxis dataKey="dept" type="category" className="text-xs" width={60} />
                  <Tooltip />
                  <Bar dataKey="count" fill="hsl(var(--primary))" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Briefcase className="h-5 w-5 text-primary" />
              Tenure Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-56">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={tenureData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis dataKey="range" className="text-xs" />
                  <YAxis className="text-xs" />
                  <Tooltip />
                  <Bar dataKey="count" fill="hsl(var(--success))" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Employee Table */}
      <Card className="hrms-card">
        <CardHeader className="pb-2">
          <CardTitle className="text-lg font-display">Employee Directory</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Employee</TableHead>
                  <TableHead>Department</TableHead>
                  <TableHead>Designation</TableHead>
                  <TableHead>Joining Date</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {employees.map((emp) => (
                  <TableRow key={emp.id}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <Avatar className="h-8 w-8">
                          <AvatarFallback className="bg-primary/10 text-primary text-xs">
                            {emp.name.split(" ").map((n) => n[0]).join("")}
                          </AvatarFallback>
                        </Avatar>
                        <div>
                          <p className="font-medium text-sm">{emp.name}</p>
                          <p className="text-xs text-muted-foreground">{emp.id}</p>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>{emp.dept}</TableCell>
                    <TableCell>{emp.designation}</TableCell>
                    <TableCell>{emp.joining}</TableCell>
                    <TableCell>{emp.type}</TableCell>
                    <TableCell>
                      <span className="px-2 py-1 rounded-full text-xs font-medium bg-success/10 text-success">
                        {emp.status}
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
