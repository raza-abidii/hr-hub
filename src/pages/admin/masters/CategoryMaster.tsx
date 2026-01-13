import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { toast } from "sonner";
import { Plus, Search, Pencil, Trash2, Tag, Users, Clock, Calendar } from "lucide-react";

interface Category {
  id: string;
  name: string;
  code: string;
  probationPeriod: number;
  noticePeriod: number;
  benefitsEligible: boolean;
  leaveEntitlement: number;
  employeeCount: number;
}

const initialCategories: Category[] = [
  { id: "1", name: "Permanent", code: "PERM", probationPeriod: 6, noticePeriod: 60, benefitsEligible: true, leaveEntitlement: 40, employeeCount: 35 },
  { id: "2", name: "Contract", code: "CONT", probationPeriod: 0, noticePeriod: 30, benefitsEligible: false, leaveEntitlement: 20, employeeCount: 8 },
  { id: "3", name: "Consultant", code: "CONS", probationPeriod: 0, noticePeriod: 15, benefitsEligible: false, leaveEntitlement: 15, employeeCount: 5 },
  { id: "4", name: "Intern", code: "INTN", probationPeriod: 1, noticePeriod: 7, benefitsEligible: false, leaveEntitlement: 10, employeeCount: 2 },
];

export default function CategoryMaster() {
  const [categories, setCategories] = useState(initialCategories);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    probationPeriod: 6,
    noticePeriod: 30,
    benefitsEligible: true,
    leaveEntitlement: 30,
  });

  const filteredCategories = categories.filter(
    (c) =>
      c.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      c.code.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = () => {
    if (!formData.name || !formData.code) {
      toast.error("Please fill all required fields");
      return;
    }

    if (editingCategory) {
      setCategories((prev) =>
        prev.map((c) =>
          c.id === editingCategory.id ? { ...c, ...formData } : c
        )
      );
      toast.success("Category updated successfully!");
    } else {
      setCategories((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData, employeeCount: 0 },
      ]);
      toast.success("Category created successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (category: Category) => {
    setEditingCategory(category);
    setFormData({
      name: category.name,
      code: category.code,
      probationPeriod: category.probationPeriod,
      noticePeriod: category.noticePeriod,
      benefitsEligible: category.benefitsEligible,
      leaveEntitlement: category.leaveEntitlement,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    const category = categories.find((c) => c.id === id);
    if (category?.employeeCount && category.employeeCount > 0) {
      toast.error("Cannot delete category with assigned employees");
      return;
    }
    setCategories((prev) => prev.filter((c) => c.id !== id));
    toast.success("Category deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingCategory(null);
    setFormData({
      name: "",
      code: "",
      probationPeriod: 6,
      noticePeriod: 30,
      benefitsEligible: true,
      leaveEntitlement: 30,
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Category Master"
          subtitle="Define employee categories"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Category
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search categories..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {filteredCategories.map((category) => (
          <Card key={category.id} className="hrms-card hover:shadow-md transition-shadow">
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    <Tag className="h-5 w-5 text-primary" />
                    <div>
                      <h3 className="font-semibold">{category.name}</h3>
                      <span className="text-xs text-muted-foreground">{category.code}</span>
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-3 text-sm">
                    <div className="flex items-center gap-2">
                      <Clock className="h-4 w-4 text-muted-foreground" />
                      <span>Probation: {category.probationPeriod} months</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Clock className="h-4 w-4 text-muted-foreground" />
                      <span>Notice: {category.noticePeriod} days</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      <span>Leave: {category.leaveEntitlement} days</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Users className="h-4 w-4 text-muted-foreground" />
                      <span>{category.employeeCount} employees</span>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {category.benefitsEligible ? (
                      <span className="px-2 py-1 rounded-full bg-success/10 text-success text-xs font-medium">
                        Benefits Eligible
                      </span>
                    ) : (
                      <span className="px-2 py-1 rounded-full bg-muted text-muted-foreground text-xs font-medium">
                        No Benefits
                      </span>
                    )}
                  </div>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon" onClick={() => handleEdit(category)}>
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-destructive"
                    onClick={() => handleDelete(category.id)}
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
              {editingCategory ? "Edit Category" : "Add New Category"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Category Name *</Label>
                <Input
                  placeholder="e.g., Permanent"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Code *</Label>
                <Input
                  placeholder="e.g., PERM"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  maxLength={4}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Probation Period (months)</Label>
                <Input
                  type="number"
                  value={formData.probationPeriod}
                  onChange={(e) => setFormData({ ...formData, probationPeriod: parseInt(e.target.value) || 0 })}
                />
              </div>
              <div className="space-y-2">
                <Label>Notice Period (days)</Label>
                <Input
                  type="number"
                  value={formData.noticePeriod}
                  onChange={(e) => setFormData({ ...formData, noticePeriod: parseInt(e.target.value) || 0 })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Leave Entitlement (days/year)</Label>
              <Input
                type="number"
                value={formData.leaveEntitlement}
                onChange={(e) => setFormData({ ...formData, leaveEntitlement: parseInt(e.target.value) || 0 })}
              />
            </div>
            <div className="flex items-center justify-between">
              <Label>Benefits Eligible</Label>
              <Switch
                checked={formData.benefitsEligible}
                onCheckedChange={(v) => setFormData({ ...formData, benefitsEligible: v })}
              />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingCategory ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
