import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
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
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Search,
  Mail,
  Phone,
  MapPin,
  Building2,
  Calendar,
  User,
  Users,
  X,
} from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

interface Employee {
  id: string;
  name: string;
  email: string;
  phone: string;
  designation: string;
  department: string;
  location: string;
  joiningDate: string;
  reportingTo: string;
  extension?: string;
  skills?: string[];
}

const mockEmployees: Employee[] = [
  {
    id: "1",
    name: "John Smith",
    email: "john.smith@sfon.com",
    phone: "+1 (555) 123-4567",
    designation: "Senior Manager",
    department: "Information Technology",
    location: "New York Office",
    joiningDate: "Jan 15, 2020",
    reportingTo: "David Wilson",
    extension: "1234",
    skills: ["Leadership", "Project Management", "Agile"],
  },
  {
    id: "2",
    name: "Sarah Johnson",
    email: "sarah.johnson@sfon.com",
    phone: "+1 (555) 234-5678",
    designation: "HR Executive",
    department: "Human Resources",
    location: "San Francisco Office",
    joiningDate: "Mar 10, 2021",
    reportingTo: "Emily Brown",
    extension: "1235",
    skills: ["Recruitment", "Employee Relations", "Training"],
  },
  {
    id: "3",
    name: "Mike Davis",
    email: "mike.davis@sfon.com",
    phone: "+1 (555) 345-6789",
    designation: "Software Developer",
    department: "Information Technology",
    location: "New York Office",
    joiningDate: "Jun 1, 2022",
    reportingTo: "John Smith",
    extension: "1236",
    skills: ["React", "TypeScript", "Node.js"],
  },
  {
    id: "4",
    name: "Emily Brown",
    email: "emily.brown@sfon.com",
    phone: "+1 (555) 456-7890",
    designation: "HR Director",
    department: "Human Resources",
    location: "Chicago Office",
    joiningDate: "Aug 20, 2019",
    reportingTo: "CEO",
    extension: "1237",
    skills: ["Strategic HR", "Policy Development", "Leadership"],
  },
  {
    id: "5",
    name: "David Wilson",
    email: "david.wilson@sfon.com",
    phone: "+1 (555) 567-8901",
    designation: "CTO",
    department: "Information Technology",
    location: "New York Office",
    joiningDate: "Feb 5, 2018",
    reportingTo: "CEO",
    extension: "1238",
    skills: ["Technology Strategy", "Architecture", "Team Building"],
  },
  {
    id: "6",
    name: "Jessica Lee",
    email: "jessica.lee@sfon.com",
    phone: "+1 (555) 678-9012",
    designation: "Marketing Manager",
    department: "Marketing",
    location: "San Francisco Office",
    joiningDate: "Apr 12, 2023",
    reportingTo: "CEO",
    extension: "1239",
    skills: ["Digital Marketing", "Brand Strategy", "Analytics"],
  },
  {
    id: "7",
    name: "Robert Chen",
    email: "robert.chen@sfon.com",
    phone: "+1 (555) 789-0123",
    designation: "Financial Analyst",
    department: "Finance",
    location: "Chicago Office",
    joiningDate: "Sep 8, 2021",
    reportingTo: "CFO",
    extension: "1240",
    skills: ["Financial Modeling", "Excel", "Data Analysis"],
  },
  {
    id: "8",
    name: "Amanda Taylor",
    email: "amanda.taylor@sfon.com",
    phone: "+1 (555) 890-1234",
    designation: "Sales Executive",
    department: "Sales",
    location: "New York Office",
    joiningDate: "Nov 15, 2022",
    reportingTo: "Sales Director",
    extension: "1241",
    skills: ["B2B Sales", "CRM", "Negotiation"],
  },
];

const departments = ["All Departments", "Information Technology", "Human Resources", "Finance", "Sales", "Marketing"];
const locations = ["All Locations", "New York Office", "San Francisco Office", "Chicago Office"];

export default function EmployeeDirectory() {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDepartment, setSelectedDepartment] = useState("All Departments");
  const [selectedLocation, setSelectedLocation] = useState("All Locations");
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const filteredEmployees = mockEmployees.filter((emp) => {
    const matchesSearch =
      emp.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.designation.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesDepartment =
      selectedDepartment === "All Departments" || emp.department === selectedDepartment;

    const matchesLocation =
      selectedLocation === "All Locations" || emp.location === selectedLocation;

    return matchesSearch && matchesDepartment && matchesLocation;
  });

  const openEmployeeDetails = (employee: Employee) => {
    setSelectedEmployee(employee);
    setIsDialogOpen(true);
  };

  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase();
  };

  const getAvatarColor = (name: string) => {
    const colors = [
      "bg-blue-500",
      "bg-purple-500",
      "bg-green-500",
      "bg-orange-500",
      "bg-pink-500",
      "bg-cyan-500",
    ];
    const index = name.charCodeAt(0) % colors.length;
    return colors[index];
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Employee Directory"
        subtitle="Find and connect with colleagues"
      />

      {/* Search & Filters */}
      <Card className="hrms-card">
        <CardContent className="p-4">
          <div className="flex flex-col md:flex-row gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by name, email, or designation..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select
              value={selectedDepartment}
              onValueChange={setSelectedDepartment}
            >
              <SelectTrigger className="w-full md:w-52">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="bg-popover">
                {departments.map((dept) => (
                  <SelectItem key={dept} value={dept}>
                    {dept}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedLocation} onValueChange={setSelectedLocation}>
              <SelectTrigger className="w-full md:w-44">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="bg-popover">
                {locations.map((loc) => (
                  <SelectItem key={loc} value={loc}>
                    {loc}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Results Count */}
      <p className="text-sm text-muted-foreground">
        Showing {filteredEmployees.length} of {mockEmployees.length} employees
      </p>

      {/* Employee Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredEmployees.map((employee) => (
          <Card
            key={employee.id}
            className="hrms-card cursor-pointer hover:border-primary transition-all"
            onClick={() => openEmployeeDetails(employee)}
          >
            <CardContent className="p-5">
              <div className="flex items-start gap-4">
                <Avatar className={`h-14 w-14 ${getAvatarColor(employee.name)}`}>
                  <AvatarFallback className="text-white font-semibold">
                    {getInitials(employee.name)}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1 min-w-0">
                  <h3 className="font-semibold truncate">{employee.name}</h3>
                  <p className="text-sm text-muted-foreground truncate">
                    {employee.designation}
                  </p>
                  <span className="inline-block mt-2 px-2 py-0.5 bg-primary/10 text-primary text-xs font-medium rounded">
                    {employee.department}
                  </span>
                </div>
              </div>

              <div className="mt-4 space-y-2">
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Mail className="h-4 w-4 flex-shrink-0" />
                  <span className="truncate">{employee.email}</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Phone className="h-4 w-4 flex-shrink-0" />
                  <span>{employee.phone}</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <MapPin className="h-4 w-4 flex-shrink-0" />
                  <span>{employee.location}</span>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t border-border flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="flex-1"
                  onClick={(e) => {
                    e.stopPropagation();
                    window.location.href = `mailto:${employee.email}`;
                  }}
                >
                  <Mail className="h-4 w-4 mr-1" />
                  Email
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="flex-1"
                  onClick={(e) => {
                    e.stopPropagation();
                    window.location.href = `tel:${employee.phone}`;
                  }}
                >
                  <Phone className="h-4 w-4 mr-1" />
                  Call
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {filteredEmployees.length === 0 && (
        <div className="text-center py-12">
          <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <p className="text-lg font-medium">No employees found</p>
          <p className="text-muted-foreground">
            Try adjusting your search or filters
          </p>
        </div>
      )}

      {/* Employee Detail Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle className="font-display">Employee Profile</DialogTitle>
          </DialogHeader>
          {selectedEmployee && (
            <div className="mt-2">
              <Tabs defaultValue="about" className="w-full">
                <TabsList className="grid w-full grid-cols-3 mb-4">
                  <TabsTrigger value="about">About</TabsTrigger>
                  <TabsTrigger value="contact">Contact</TabsTrigger>
                  <TabsTrigger value="skills">Skills</TabsTrigger>
                </TabsList>

                <TabsContent value="about" className="space-y-4">
                  <div className="flex items-center gap-4">
                    <Avatar className={`h-16 w-16 ${getAvatarColor(selectedEmployee.name)}`}>
                      <AvatarFallback className="text-white text-xl font-semibold">
                        {getInitials(selectedEmployee.name)}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <h3 className="text-xl font-semibold">{selectedEmployee.name}</h3>
                      <p className="text-muted-foreground">{selectedEmployee.designation}</p>
                    </div>
                  </div>

                  <div className="grid gap-3 p-4 bg-muted/50 rounded-lg">
                    <div className="flex items-center gap-3">
                      <Building2 className="h-4 w-4 text-primary" />
                      <span className="text-sm">{selectedEmployee.department}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <MapPin className="h-4 w-4 text-primary" />
                      <span className="text-sm">{selectedEmployee.location}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <Calendar className="h-4 w-4 text-primary" />
                      <span className="text-sm">Joined {selectedEmployee.joiningDate}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <User className="h-4 w-4 text-primary" />
                      <span className="text-sm">Reports to {selectedEmployee.reportingTo}</span>
                    </div>
                  </div>
                </TabsContent>

                <TabsContent value="contact" className="space-y-4">
                  <div className="grid gap-3 p-4 bg-muted/50 rounded-lg">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Mail className="h-4 w-4 text-primary" />
                        <span className="text-sm">{selectedEmployee.email}</span>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => window.location.href = `mailto:${selectedEmployee.email}`}
                      >
                        <Mail className="h-4 w-4" />
                      </Button>
                    </div>
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Phone className="h-4 w-4 text-primary" />
                        <span className="text-sm">{selectedEmployee.phone}</span>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => window.location.href = `tel:${selectedEmployee.phone}`}
                      >
                        <Phone className="h-4 w-4" />
                      </Button>
                    </div>
                    {selectedEmployee.extension && (
                      <div className="flex items-center gap-3">
                        <Phone className="h-4 w-4 text-primary" />
                        <span className="text-sm">Ext: {selectedEmployee.extension}</span>
                      </div>
                    )}
                  </div>
                </TabsContent>

                <TabsContent value="skills" className="space-y-4">
                  <div className="p-4 bg-muted/50 rounded-lg">
                    <p className="text-sm font-medium mb-3">Professional Skills</p>
                    <div className="flex flex-wrap gap-2">
                      {selectedEmployee.skills?.map((skill) => (
                        <span
                          key={skill}
                          className="px-3 py-1 bg-primary/10 text-primary text-sm rounded-full"
                        >
                          {skill}
                        </span>
                      ))}
                    </div>
                  </div>
                </TabsContent>
              </Tabs>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
