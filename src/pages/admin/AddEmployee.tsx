import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Save, Upload, User, Briefcase, CreditCard, Phone, FileText } from "lucide-react";
import { useToast } from "@/hooks/use-toast";

// Master data - should come from master tables
const departments = [
  { id: 1, name: "Information Technology", code: "IT" },
  { id: 2, name: "Human Resources", code: "HR" },
  { id: 3, name: "Finance & Accounts", code: "FIN" },
  { id: 4, name: "Sales & Marketing", code: "SALES" },
  { id: 5, name: "Operations", code: "OPS" },
  { id: 6, name: "Research & Development", code: "R&D" },
];

const designations = [
  { id: 1, name: "Chief Executive Officer", grade: "L1" },
  { id: 2, name: "Chief Technology Officer", grade: "L1" },
  { id: 3, name: "Senior Manager", grade: "L2" },
  { id: 4, name: "Manager", grade: "L3" },
  { id: 5, name: "Team Lead", grade: "L4" },
  { id: 6, name: "Senior Developer", grade: "L5" },
  { id: 7, name: "Developer", grade: "L6" },
  { id: 8, name: "Junior Developer", grade: "L7" },
  { id: 9, name: "Intern", grade: "L8" },
];

const categories = [
  { id: 1, name: "Permanent", code: "PERM" },
  { id: 2, name: "Contract", code: "CONT" },
  { id: 3, name: "Consultant", code: "CONS" },
  { id: 4, name: "Probation", code: "PROB" },
  { id: 5, name: "Trainee", code: "TRAIN" },
];

const shifts = [
  { id: 1, name: "General Shift", timing: "09:00 AM - 06:00 PM" },
  { id: 2, name: "Morning Shift", timing: "06:00 AM - 03:00 PM" },
  { id: 3, name: "Evening Shift", timing: "03:00 PM - 12:00 AM" },
  { id: 4, name: "Night Shift", timing: "10:00 PM - 07:00 AM" },
  { id: 5, name: "Flexible", timing: "Flexible Hours" },
];

const leaveTypes = [
  { id: 1, name: "Casual Leave", code: "CL", days: 12 },
  { id: 2, name: "Sick Leave", code: "SL", days: 10 },
  { id: 3, name: "Earned Leave", code: "EL", days: 15 },
  { id: 4, name: "Maternity Leave", code: "ML", days: 180 },
  { id: 5, name: "Paternity Leave", code: "PL", days: 15 },
];

const companies = [
  { id: 1, name: "SFON Technologies Pvt Ltd", code: "SFON" },
  { id: 2, name: "SFON Solutions Inc", code: "SFON-US" },
  { id: 3, name: "SFON Global Services", code: "SFON-GL" },
];

const machines = [
  { id: 1, name: "Main Entrance Biometric", location: "Building A" },
  { id: 2, name: "IT Floor Biometric", location: "3rd Floor" },
  { id: 3, name: "HR Floor Biometric", location: "2nd Floor" },
];

const holidays = [
  { id: 1, name: "New Year's Day", date: "2024-01-01" },
  { id: 2, name: "Republic Day", date: "2024-01-26" },
  { id: 3, name: "Holi", date: "2024-03-25" },
  { id: 4, name: "Independence Day", date: "2024-08-15" },
  { id: 5, name: "Diwali", date: "2024-11-01" },
  { id: 6, name: "Christmas", date: "2024-12-25" },
];

const genders = ["Male", "Female", "Other"];
const relations = ["Spouse", "Parent", "Sibling", "Child", "Friend", "Other"];
const bloodGroups = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];
const maritalStatus = ["Single", "Married", "Divorced", "Widowed"];
const employmentTypes = ["Full-time", "Part-time", "Contract", "Remote"];

export default function AddEmployee() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [formData, setFormData] = useState({
    // Personal
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    dateOfBirth: "",
    gender: "",
    bloodGroup: "",
    maritalStatus: "",
    address: "",
    city: "",
    state: "",
    zipCode: "",
    country: "India",
    // Employment
    employeeId: "",
    joiningDate: "",
    department: "",
    designation: "",
    category: "",
    shift: "",
    company: "",
    machine: "",
    reportingTo: "",
    employmentType: "",
    workLocation: "",
    // Compensation
    basicSalary: "",
    allowances: "",
    // Emergency
    emergencyContactName: "",
    emergencyContactPhone: "",
    emergencyContactRelation: "",
    // Documents
    aadhaarNumber: "",
    panNumber: "",
    bankAccountNumber: "",
    ifscCode: "",
    bankName: "",
  });

  const updateField = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = () => {
    // Check required fields
    const requiredFields = [
      "firstName", "lastName", "email", "phone", "dateOfBirth", "gender",
      "employeeId", "joiningDate", "department", "designation"
    ];

    const missingFields = requiredFields.filter(
      (field) => !formData[field as keyof typeof formData]
    );

    if (missingFields.length > 0) {
      toast({
        title: "Validation Error",
        description: "Please fill all required fields",
        variant: "destructive",
      });
      return;
    }

    toast({
      title: "Success!",
      description: "Employee added successfully",
    });

    navigate("/admin/masters/employee");
  };

  const getInitials = () => {
    const first = formData.firstName.charAt(0).toUpperCase();
    const last = formData.lastName.charAt(0).toUpperCase();
    return first + last || "?";
  };

  return (
    <div className="space-y-6 animate-fade-in pb-24">
      <PageHeader
        title="Add New Employee"
        subtitle="Fill in the details to add a new employee"
        showBack
        backPath="/admin/masters/employee"
      />

      {/* Profile Picture */}
      <Card className="hrms-card">
        <CardContent className="p-6">
          <div className="flex items-center gap-6">
            <Avatar className="h-24 w-24 border-4 border-primary/20">
              <AvatarFallback className="gradient-primary text-primary-foreground text-2xl font-bold">
                {getInitials()}
              </AvatarFallback>
            </Avatar>
            <div>
              <Button variant="outline">
                <Upload className="h-4 w-4 mr-2" />
                Upload Photo
              </Button>
              <p className="text-xs text-muted-foreground mt-2">
                JPG, PNG or GIF. Max size 2MB
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Personal Information */}
      <Card className="hrms-card">
        <CardHeader>
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <User className="h-5 w-5 text-primary" />
            Personal Information
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label>First Name *</Label>
            <Input
              placeholder="Enter first name"
              value={formData.firstName}
              onChange={(e) => updateField("firstName", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Last Name *</Label>
            <Input
              placeholder="Enter last name"
              value={formData.lastName}
              onChange={(e) => updateField("lastName", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Email Address *</Label>
            <Input
              type="email"
              placeholder="email@example.com"
              value={formData.email}
              onChange={(e) => updateField("email", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Phone Number *</Label>
            <Input
              placeholder="+1 (555) 123-4567"
              value={formData.phone}
              onChange={(e) => updateField("phone", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Date of Birth *</Label>
            <Input
              type="date"
              value={formData.dateOfBirth}
              onChange={(e) => updateField("dateOfBirth", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Gender *</Label>
            <Select
              value={formData.gender}
              onValueChange={(v) => updateField("gender", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select gender" />
              </SelectTrigger>
              <SelectContent>
                {genders.map((g) => (
                  <SelectItem key={g} value={g}>{g}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Blood Group</Label>
            <Select
              value={formData.bloodGroup}
              onValueChange={(v) => updateField("bloodGroup", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select blood group" />
              </SelectTrigger>
              <SelectContent>
                {bloodGroups.map((bg) => (
                  <SelectItem key={bg} value={bg}>{bg}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Marital Status</Label>
            <Select
              value={formData.maritalStatus}
              onValueChange={(v) => updateField("maritalStatus", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select status" />
              </SelectTrigger>
              <SelectContent>
                {maritalStatus.map((ms) => (
                  <SelectItem key={ms} value={ms}>{ms}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2 md:col-span-2">
            <Label>Address</Label>
            <Input
              placeholder="Street address"
              value={formData.address}
              onChange={(e) => updateField("address", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>City</Label>
            <Input
              placeholder="City"
              value={formData.city}
              onChange={(e) => updateField("city", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>State</Label>
            <Input
              placeholder="State"
              value={formData.state}
              onChange={(e) => updateField("state", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>ZIP Code</Label>
            <Input
              placeholder="12345"
              value={formData.zipCode}
              onChange={(e) => updateField("zipCode", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Country</Label>
            <Input
              placeholder="Country"
              value={formData.country}
              onChange={(e) => updateField("country", e.target.value)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Employment Information */}
      <Card className="hrms-card">
        <CardHeader>
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <Briefcase className="h-5 w-5 text-primary" />
            Employment Information
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label>Employee ID *</Label>
            <Input
              placeholder="EMP001"
              value={formData.employeeId}
              onChange={(e) => updateField("employeeId", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Joining Date *</Label>
            <Input
              type="date"
              value={formData.joiningDate}
              onChange={(e) => updateField("joiningDate", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Department *</Label>
            <Select
              value={formData.department}
              onValueChange={(v) => updateField("department", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select department" />
              </SelectTrigger>
              <SelectContent>
                {departments.map((d) => (
                  <SelectItem key={d.id.toString()} value={d.name}>
                    {d.name} ({d.code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Designation *</Label>
            <Select
              value={formData.designation}
              onValueChange={(v) => updateField("designation", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select designation" />
              </SelectTrigger>
              <SelectContent>
                {designations.map((d) => (
                  <SelectItem key={d.id.toString()} value={d.name}>
                    {d.name} - {d.grade}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Category</Label>
            <Select
              value={formData.category}
              onValueChange={(v) => updateField("category", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select category" />
              </SelectTrigger>
              <SelectContent>
                {categories.map((c) => (
                  <SelectItem key={c.id.toString()} value={c.name}>
                    {c.name} ({c.code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Shift</Label>
            <Select
              value={formData.shift}
              onValueChange={(v) => updateField("shift", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select shift" />
              </SelectTrigger>
              <SelectContent>
                {shifts.map((s) => (
                  <SelectItem key={s.id.toString()} value={s.name}>
                    {s.name} ({s.timing})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Company</Label>
            <Select
              value={formData.company}
              onValueChange={(v) => updateField("company", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select company" />
              </SelectTrigger>
              <SelectContent>
                {companies.map((c) => (
                  <SelectItem key={c.id.toString()} value={c.name}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Attendance Machine</Label>
            <Select
              value={formData.machine}
              onValueChange={(v) => updateField("machine", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select machine" />
              </SelectTrigger>
              <SelectContent>
                {machines.map((m) => (
                  <SelectItem key={m.id.toString()} value={m.name}>
                    {m.name} - {m.location}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Employment Type</Label>
            <Select
              value={formData.employmentType}
              onValueChange={(v) => updateField("employmentType", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select type" />
              </SelectTrigger>
              <SelectContent>
                {employmentTypes.map((et) => (
                  <SelectItem key={et} value={et}>{et}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>Work Location</Label>
            <Input
              placeholder="Office location"
              value={formData.workLocation}
              onChange={(e) => updateField("workLocation", e.target.value)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Compensation */}
      <Card className="hrms-card">
        <CardHeader>
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <CreditCard className="h-5 w-5 text-primary" />
            Compensation Details
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label>Basic Salary</Label>
            <Input
              type="number"
              placeholder="Monthly base pay"
              value={formData.basicSalary}
              onChange={(e) => updateField("basicSalary", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Allowances</Label>
            <Input
              type="number"
              placeholder="Additional benefits"
              value={formData.allowances}
              onChange={(e) => updateField("allowances", e.target.value)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Emergency Contact */}
      <Card className="hrms-card">
        <CardHeader>
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <Phone className="h-5 w-5 text-primary" />
            Emergency Contact
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="space-y-2">
            <Label>Contact Name</Label>
            <Input
              placeholder="Full name"
              value={formData.emergencyContactName}
              onChange={(e) => updateField("emergencyContactName", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Contact Phone</Label>
            <Input
              placeholder="Phone number"
              value={formData.emergencyContactPhone}
              onChange={(e) => updateField("emergencyContactPhone", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Relation</Label>
            <Select
              value={formData.emergencyContactRelation}
              onValueChange={(v) => updateField("emergencyContactRelation", v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select relation" />
              </SelectTrigger>
              <SelectContent>
                {relations.map((r) => (
                  <SelectItem key={r} value={r}>{r}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Bank & Documents */}
      <Card className="hrms-card">
        <CardHeader>
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <FileText className="h-5 w-5 text-primary" />
            Bank & Document Details
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label>Aadhaar Number</Label>
            <Input
              placeholder="1234 5678 9012"
              value={formData.aadhaarNumber}
              onChange={(e) => updateField("aadhaarNumber", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>PAN Number</Label>
            <Input
              placeholder="ABCDE1234F"
              value={formData.panNumber}
              onChange={(e) => updateField("panNumber", e.target.value.toUpperCase())}
            />
          </div>
          <div className="space-y-2">
            <Label>Bank Account Number</Label>
            <Input
              placeholder="1234567890"
              value={formData.bankAccountNumber}
              onChange={(e) => updateField("bankAccountNumber", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>IFSC Code</Label>
            <Input
              placeholder="SBIN0001234"
              value={formData.ifscCode}
              onChange={(e) => updateField("ifscCode", e.target.value.toUpperCase())}
            />
          </div>
          <div className="space-y-2 md:col-span-2">
            <Label>Bank Name</Label>
            <Input
              placeholder="State Bank of India"
              value={formData.bankName}
              onChange={(e) => updateField("bankName", e.target.value)}
            />
          </div>
        </CardContent>
      </Card>

      {/* Sticky Action Bar */}
      <div className="fixed bottom-0 left-0 right-0 lg:left-72 bg-card border-t border-border p-4 shadow-elevated z-20">
        <div className="container max-w-4xl mx-auto flex gap-3">
          <Button
            variant="outline"
            className="flex-1"
            onClick={() => navigate("/admin/masters/employee")}
          >
            Cancel
          </Button>
          <Button className="flex-1 gradient-primary" onClick={handleSubmit}>
            <Save className="h-4 w-4 mr-2" />
            Add Employee
          </Button>
        </div>
      </div>
    </div>
  );
}
