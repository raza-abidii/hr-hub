import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { PageHeader } from "@/components/ui/page-header";
import { StatusBadge } from "@/components/ui/status-badge";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Plus, Search, Edit, Eye, Users, UserPlus, Clock, AlertTriangle } from "lucide-react";
import { StatCard } from "@/components/ui/stat-card";

interface Employee {
  id: string;
  employeeId: string;
  name: string;
  email: string;
  phone: string;
  department: string;
  designation: string;
  joiningDate: string;
  status: 'active' | 'inactive';
  avatar?: string;
}

const mockEmployees: Employee[] = [
  {
    id: "1",
    employeeId: "EMP001",
    name: "Sarah Wilson",
    email: "sarah.wilson@sfon.com",
    phone: "+1 555-123-4567",
    department: "Information Technology",
    designation: "Senior Developer",
    joiningDate: "Mar 15, 2023",
    status: "active",
  },
  {
    id: "2",
    employeeId: "EMP002",
    name: "Tom Brown",
    email: "tom.brown@sfon.com",
    phone: "+1 555-234-5678",
    department: "Human Resources",
    designation: "HR Manager",
    joiningDate: "Jan 10, 2022",
    status: "active",
  },
  {
    id: "3",
    employeeId: "EMP003",
    name: "Emily Davis",
    email: "emily.davis@sfon.com",
    phone: "+1 555-345-6789",
    department: "Finance",
    designation: "Financial Analyst",
    joiningDate: "Jun 1, 2024",
    status: "active",
  },
  {
    id: "4",
    employeeId: "EMP004",
    name: "Mike Johnson",
    email: "mike.johnson@sfon.com",
    phone: "+1 555-456-7890",
    department: "Sales",
    designation: "Sales Executive",
    joiningDate: "Sep 20, 2023",
    status: "inactive",
  },
  {
    id: "5",
    employeeId: "EMP005",
    name: "Jessica Lee",
    email: "jessica.lee@sfon.com",
    phone: "+1 555-567-8901",
    department: "Marketing",
    designation: "Marketing Manager",
    joiningDate: "Feb 28, 2024",
    status: "active",
  },
];

const departments = ["All Departments", "Information Technology", "Human Resources", "Finance", "Sales", "Marketing"];
const statuses = ["All Status", "Active", "Inactive"];

export default function EmployeeMaster() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDepartment, setSelectedDepartment] = useState("All Departments");
  const [selectedStatus, setSelectedStatus] = useState("All Status");

  const filteredEmployees = mockEmployees.filter((emp) => {
    const matchesSearch =
      emp.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.email.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesDepartment =
      selectedDepartment === "All Departments" || emp.department === selectedDepartment;

    const matchesStatus =
      selectedStatus === "All Status" ||
      emp.status === selectedStatus.toLowerCase();

    return matchesSearch && matchesDepartment && matchesStatus;
  });

  const activeCount = mockEmployees.filter(e => e.status === 'active').length;
  const newThisMonth = 2; // Mock value
  const onProbation = 3; // Mock value

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Employee Master"
        subtitle="Manage employee information"
        action={
          <Button
            className="gradient-primary"
            onClick={() => navigate("/admin/add-employee")}
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Employee
          </Button>
        }
      />

      {/* Quick Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="Total Employees"
          value={mockEmployees.length}
          icon={Users}
          variant="default"
        />
        <StatCard
          title="Active"
          value={activeCount}
          icon={Users}
          variant="success"
        />
        <StatCard
          title="New This Month"
          value={newThisMonth}
          icon={UserPlus}
          variant="info"
        />
        <StatCard
          title="On Probation"
          value={onProbation}
          icon={Clock}
          variant="warning"
        />
      </div>

      {/* Filters */}
      <Card className="hrms-card">
        <CardContent className="p-4">
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by name, ID, or email..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select
              value={selectedDepartment}
              onValueChange={setSelectedDepartment}
            >
              <SelectTrigger className="w-full sm:w-48">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {departments.map((dept) => (
                  <SelectItem key={dept} value={dept}>
                    {dept}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedStatus} onValueChange={setSelectedStatus}>
              <SelectTrigger className="w-full sm:w-36">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {statuses.map((status) => (
                  <SelectItem key={status} value={status}>
                    {status}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Employee List */}
      <Card className="hrms-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border bg-muted/50">
                <th className="text-left p-4 font-medium text-sm text-muted-foreground">
                  Employee
                </th>
                <th className="text-left p-4 font-medium text-sm text-muted-foreground hidden md:table-cell">
                  Department
                </th>
                <th className="text-left p-4 font-medium text-sm text-muted-foreground hidden lg:table-cell">
                  Designation
                </th>
                <th className="text-left p-4 font-medium text-sm text-muted-foreground hidden lg:table-cell">
                  Joining Date
                </th>
                <th className="text-left p-4 font-medium text-sm text-muted-foreground">
                  Status
                </th>
                <th className="text-right p-4 font-medium text-sm text-muted-foreground">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredEmployees.map((employee) => (
                <tr
                  key={employee.id}
                  className="border-b border-border last:border-0 hover:bg-muted/30 transition-colors"
                >
                  <td className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <span className="text-sm font-semibold text-primary">
                          {employee.name.charAt(0)}
                        </span>
                      </div>
                      <div className="min-w-0">
                        <p className="font-medium text-sm truncate">
                          {employee.name}
                        </p>
                        <p className="text-xs text-muted-foreground truncate">
                          {employee.employeeId} • {employee.email}
                        </p>
                      </div>
                    </div>
                  </td>
                  <td className="p-4 hidden md:table-cell">
                    <span className="text-sm">{employee.department}</span>
                  </td>
                  <td className="p-4 hidden lg:table-cell">
                    <span className="text-sm">{employee.designation}</span>
                  </td>
                  <td className="p-4 hidden lg:table-cell">
                    <span className="text-sm text-muted-foreground">
                      {employee.joiningDate}
                    </span>
                  </td>
                  <td className="p-4">
                    <StatusBadge
                      variant={employee.status}
                      showIcon={false}
                    />
                  </td>
                  <td className="p-4">
                    <div className="flex items-center justify-end gap-1">
                      <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Edit className="h-4 w-4" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {filteredEmployees.length === 0 && (
          <div className="p-8 text-center text-muted-foreground">
            No employees found matching your criteria.
          </div>
        )}
      </Card>
    </div>
  );
}
