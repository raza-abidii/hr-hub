import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Plus, Search, Edit, Trash2, Users } from "lucide-react";
import { useToast } from "@/hooks/use-toast";

interface Department {
  id: string;
  name: string;
  code: string;
  description: string;
  employeeCount: number;
}

const mockDepartments: Department[] = [
  {
    id: "1",
    name: "Human Resources",
    code: "HR",
    description: "Employee management and relations",
    employeeCount: 8,
  },
  {
    id: "2",
    name: "Information Technology",
    code: "IT",
    description: "Software development and support",
    employeeCount: 45,
  },
  {
    id: "3",
    name: "Sales & Marketing",
    code: "SM",
    description: "Sales and marketing operations",
    employeeCount: 32,
  },
  {
    id: "4",
    name: "Finance",
    code: "FIN",
    description: "Financial management and accounting",
    employeeCount: 12,
  },
];

export default function DepartmentMaster() {
  const [departments, setDepartments] = useState<Department[]>(mockDepartments);
  const [searchQuery, setSearchQuery] = useState("");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingDepartment, setEditingDepartment] = useState<Department | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    description: "",
  });
  const { toast } = useToast();

  const filteredDepartments = departments.filter(
    (dept) =>
      dept.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      dept.code.toLowerCase().includes(searchQuery.toLowerCase()) ||
      dept.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const openAddDialog = () => {
    setEditingDepartment(null);
    setFormData({ name: "", code: "", description: "" });
    setIsDialogOpen(true);
  };

  const openEditDialog = (dept: Department) => {
    setEditingDepartment(dept);
    setFormData({
      name: dept.name,
      code: dept.code,
      description: dept.description,
    });
    setIsDialogOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name.trim() || !formData.code.trim()) {
      toast({
        title: "Validation Error",
        description: "Please fill all required fields",
        variant: "destructive",
      });
      return;
    }

    if (editingDepartment) {
      setDepartments((prev) =>
        prev.map((d) =>
          d.id === editingDepartment.id
            ? { ...d, ...formData }
            : d
        )
      );
      toast({
        title: "Success!",
        description: "Department updated successfully",
      });
    } else {
      const newDept: Department = {
        id: String(Date.now()),
        ...formData,
        employeeCount: 0,
      };
      setDepartments((prev) => [...prev, newDept]);
      toast({
        title: "Success!",
        description: "Department created successfully",
      });
    }

    setIsDialogOpen(false);
    setFormData({ name: "", code: "", description: "" });
  };

  const handleDelete = (dept: Department) => {
    if (dept.employeeCount > 0) {
      toast({
        title: "Cannot Delete",
        description: "Cannot delete department with assigned employees",
        variant: "destructive",
      });
      return;
    }

    setDepartments((prev) => prev.filter((d) => d.id !== dept.id));
    toast({
      title: "Deleted",
      description: "Department deleted successfully",
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Department Master"
        subtitle="Manage organizational departments"
        action={
          <Button className="gradient-primary" onClick={openAddDialog}>
            <Plus className="h-4 w-4 mr-2" />
            Add Department
          </Button>
        }
      />

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search departments..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Department Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {filteredDepartments.map((dept) => (
          <Card
            key={dept.id}
            className="overflow-hidden hover:shadow-elevated transition-shadow"
          >
            <div className="h-2 gradient-primary" />
            <CardContent className="p-5">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="font-display font-semibold text-lg">
                    {dept.name}
                  </h3>
                  <span className="inline-block mt-1 px-2 py-0.5 bg-secondary text-secondary-foreground text-xs font-medium rounded">
                    {dept.code}
                  </span>
                </div>
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() => openEditDialog(dept)}
                  >
                    <Edit className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-destructive hover:text-destructive"
                    onClick={() => handleDelete(dept)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
              <p className="text-sm text-muted-foreground mb-4">
                {dept.description}
              </p>
              <div className="flex items-center gap-2 text-primary">
                <Users className="h-4 w-4" />
                <span className="text-sm font-medium">
                  {dept.employeeCount} Employees
                </span>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {filteredDepartments.length === 0 && (
        <div className="text-center py-12 text-muted-foreground">
          No departments found.
        </div>
      )}

      {/* Add/Edit Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="font-display">
              {editingDepartment ? "Edit Department" : "Add Department"}
            </DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4 mt-4">
            <div className="space-y-2">
              <Label>Department Name *</Label>
              <Input
                placeholder="e.g., Operations"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
              />
            </div>
            <div className="space-y-2">
              <Label>Department Code *</Label>
              <Input
                placeholder="e.g., OPS"
                value={formData.code}
                onChange={(e) =>
                  setFormData({ ...formData, code: e.target.value.toUpperCase() })
                }
                maxLength={5}
              />
            </div>
            <div className="space-y-2">
              <Label>Description</Label>
              <Input
                placeholder="Brief description"
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
              />
            </div>
            <div className="flex gap-3 pt-2">
              <Button
                type="button"
                variant="outline"
                className="flex-1"
                onClick={() => setIsDialogOpen(false)}
              >
                Cancel
              </Button>
              <Button type="submit" className="flex-1 gradient-primary">
                {editingDepartment ? "Update" : "Save"}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
