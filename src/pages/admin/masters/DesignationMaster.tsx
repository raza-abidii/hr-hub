import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent } from "@/components/ui/card";
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
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { toast } from "sonner";
import { Plus, Search, Pencil, Trash2, Briefcase, Users } from "lucide-react";

interface Designation {
  id: string;
  name: string;
  code: string;
  department: string;
  grade: string;
  employeeCount: number;
}

const initialDesignations: Designation[] = [
  { id: "1", name: "Software Developer", code: "SD", department: "IT", grade: "Mid", employeeCount: 12 },
  { id: "2", name: "Senior Manager", code: "SM", department: "HR", grade: "Manager", employeeCount: 3 },
  { id: "3", name: "Sales Executive", code: "SE", department: "Sales", grade: "Junior", employeeCount: 15 },
  { id: "4", name: "Financial Analyst", code: "FA", department: "Finance", grade: "Mid", employeeCount: 8 },
  { id: "5", name: "Marketing Lead", code: "ML", department: "Marketing", grade: "Senior", employeeCount: 4 },
];

const departments = ["IT", "HR", "Sales", "Finance", "Marketing"];
const grades = ["Junior", "Mid", "Senior", "Manager", "Director"];

export default function DesignationMaster() {
  const [designations, setDesignations] = useState(initialDesignations);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingDesignation, setEditingDesignation] = useState<Designation | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    department: "",
    grade: "",
  });

  const filteredDesignations = designations.filter(
    (d) =>
      d.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      d.code.toLowerCase().includes(searchQuery.toLowerCase()) ||
      d.department.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = () => {
    if (!formData.name || !formData.code || !formData.department || !formData.grade) {
      toast.error("Please fill all required fields");
      return;
    }

    if (editingDesignation) {
      setDesignations((prev) =>
        prev.map((d) =>
          d.id === editingDesignation.id ? { ...d, ...formData } : d
        )
      );
      toast.success("Designation updated successfully!");
    } else {
      setDesignations((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData, employeeCount: 0 },
      ]);
      toast.success("Designation created successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (designation: Designation) => {
    setEditingDesignation(designation);
    setFormData({
      name: designation.name,
      code: designation.code,
      department: designation.department,
      grade: designation.grade,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    const designation = designations.find((d) => d.id === id);
    if (designation?.employeeCount && designation.employeeCount > 0) {
      toast.error("Cannot delete designation with assigned employees");
      return;
    }
    setDesignations((prev) => prev.filter((d) => d.id !== id));
    toast.success("Designation deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingDesignation(null);
    setFormData({ name: "", code: "", department: "", grade: "" });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Designation Master"
          subtitle="Manage job titles and positions"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Designation
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search designations..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {filteredDesignations.map((designation) => (
          <Card key={designation.id} className="hrms-card overflow-hidden">
            <div className="h-2 bg-gradient-to-r from-primary to-primary/60" />
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Briefcase className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">{designation.name}</h3>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <span className="px-2 py-1 rounded-full bg-primary/10 text-primary text-xs font-medium">
                      {designation.code}
                    </span>
                    <span className="px-2 py-1 rounded-full bg-muted text-muted-foreground text-xs font-medium">
                      {designation.department}
                    </span>
                    <span className="px-2 py-1 rounded-full bg-success/10 text-success text-xs font-medium">
                      {designation.grade}
                    </span>
                  </div>
                  <div className="flex items-center gap-1 text-sm text-muted-foreground">
                    <Users className="h-4 w-4" />
                    <span>{designation.employeeCount} employees</span>
                  </div>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon" onClick={() => handleEdit(designation)}>
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-destructive"
                    onClick={() => handleDelete(designation.id)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog open={showDialog} onOpenChange={handleCloseDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingDesignation ? "Edit Designation" : "Add New Designation"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Designation Name *</Label>
              <Input
                placeholder="e.g., Project Manager"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label>Designation Code *</Label>
              <Input
                placeholder="e.g., PM"
                value={formData.code}
                onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                maxLength={5}
              />
            </div>
            <div className="space-y-2">
              <Label>Department *</Label>
              <Select value={formData.department} onValueChange={(v) => setFormData({ ...formData, department: v })}>
                <SelectTrigger>
                  <SelectValue placeholder="Select department" />
                </SelectTrigger>
                <SelectContent>
                  {departments.map((d) => (
                    <SelectItem key={d} value={d}>{d}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Grade/Level *</Label>
              <Select value={formData.grade} onValueChange={(v) => setFormData({ ...formData, grade: v })}>
                <SelectTrigger>
                  <SelectValue placeholder="Select grade" />
                </SelectTrigger>
                <SelectContent>
                  {grades.map((g) => (
                    <SelectItem key={g} value={g}>{g}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingDesignation ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
