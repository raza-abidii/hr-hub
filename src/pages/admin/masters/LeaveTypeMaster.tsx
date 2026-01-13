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
import { Plus, Search, Pencil, Trash2, Calendar, Check, ArrowRight, Banknote } from "lucide-react";

interface LeaveType {
  id: string;
  name: string;
  code: string;
  annualAllocation: number;
  carryForward: boolean;
  maxCarryForward: number;
  encashable: boolean;
  requiresApproval: boolean;
  color: string;
}

const initialLeaveTypes: LeaveType[] = [
  { id: "1", name: "Casual Leave", code: "CL", annualAllocation: 12, carryForward: true, maxCarryForward: 5, encashable: false, requiresApproval: true, color: "#14b8a6" },
  { id: "2", name: "Sick Leave", code: "SL", annualAllocation: 10, carryForward: false, maxCarryForward: 0, encashable: false, requiresApproval: true, color: "#f59e0b" },
  { id: "3", name: "Earned Leave", code: "EL", annualAllocation: 18, carryForward: true, maxCarryForward: 10, encashable: true, requiresApproval: true, color: "#10b981" },
  { id: "4", name: "Compensatory Off", code: "CO", annualAllocation: 0, carryForward: false, maxCarryForward: 0, encashable: false, requiresApproval: true, color: "#6366f1" },
  { id: "5", name: "Paternity Leave", code: "PL", annualAllocation: 7, carryForward: false, maxCarryForward: 0, encashable: false, requiresApproval: true, color: "#3b82f6" },
];

export default function LeaveTypeMaster() {
  const [leaveTypes, setLeaveTypes] = useState(initialLeaveTypes);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingLeaveType, setEditingLeaveType] = useState<LeaveType | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    annualAllocation: 12,
    carryForward: false,
    maxCarryForward: 0,
    encashable: false,
    requiresApproval: true,
    color: "#14b8a6",
  });

  const filteredLeaveTypes = leaveTypes.filter(
    (l) =>
      l.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      l.code.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = () => {
    if (!formData.name || !formData.code) {
      toast.error("Please fill all required fields");
      return;
    }

    if (editingLeaveType) {
      setLeaveTypes((prev) =>
        prev.map((l) =>
          l.id === editingLeaveType.id ? { ...l, ...formData } : l
        )
      );
      toast.success("Leave type updated successfully!");
    } else {
      setLeaveTypes((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData },
      ]);
      toast.success("Leave type created successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (leaveType: LeaveType) => {
    setEditingLeaveType(leaveType);
    setFormData({
      name: leaveType.name,
      code: leaveType.code,
      annualAllocation: leaveType.annualAllocation,
      carryForward: leaveType.carryForward,
      maxCarryForward: leaveType.maxCarryForward,
      encashable: leaveType.encashable,
      requiresApproval: leaveType.requiresApproval,
      color: leaveType.color,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    setLeaveTypes((prev) => prev.filter((l) => l.id !== id));
    toast.success("Leave type deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingLeaveType(null);
    setFormData({
      name: "",
      code: "",
      annualAllocation: 12,
      carryForward: false,
      maxCarryForward: 0,
      encashable: false,
      requiresApproval: true,
      color: "#14b8a6",
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Leave Type Master"
          subtitle="Configure leave types and policies"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Leave Type
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search leave types..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredLeaveTypes.map((leaveType) => (
          <Card key={leaveType.id} className="hrms-card overflow-hidden hover:shadow-md transition-shadow">
            <div className="h-2" style={{ backgroundColor: leaveType.color }} />
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    <Calendar className="h-5 w-5" style={{ color: leaveType.color }} />
                    <div>
                      <h3 className="font-semibold">{leaveType.name}</h3>
                      <span className="text-xs text-muted-foreground">{leaveType.code}</span>
                    </div>
                  </div>
                  <div className="text-2xl font-bold" style={{ color: leaveType.color }}>
                    {leaveType.annualAllocation} <span className="text-sm font-normal text-muted-foreground">days/year</span>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {leaveType.carryForward && (
                      <span className="px-2 py-1 rounded-full bg-success/10 text-success text-xs font-medium flex items-center gap-1">
                        <ArrowRight className="h-3 w-3" />
                        Carry: {leaveType.maxCarryForward}
                      </span>
                    )}
                    {leaveType.encashable && (
                      <span className="px-2 py-1 rounded-full bg-warning/10 text-warning text-xs font-medium flex items-center gap-1">
                        <Banknote className="h-3 w-3" />
                        Encashable
                      </span>
                    )}
                    {leaveType.requiresApproval && (
                      <span className="px-2 py-1 rounded-full bg-info/10 text-info text-xs font-medium flex items-center gap-1">
                        <Check className="h-3 w-3" />
                        Approval
                      </span>
                    )}
                  </div>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon" onClick={() => handleEdit(leaveType)}>
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-destructive"
                    onClick={() => handleDelete(leaveType.id)}
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
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {editingLeaveType ? "Edit Leave Type" : "Add New Leave Type"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Leave Type Name *</Label>
                <Input
                  placeholder="e.g., Casual Leave"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Code *</Label>
                <Input
                  placeholder="e.g., CL"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  maxLength={3}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Annual Allocation (days)</Label>
                <Input
                  type="number"
                  value={formData.annualAllocation}
                  onChange={(e) => setFormData({ ...formData, annualAllocation: parseInt(e.target.value) || 0 })}
                />
              </div>
              <div className="space-y-2">
                <Label>Color</Label>
                <Input
                  type="color"
                  value={formData.color}
                  onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                  className="h-10 p-1"
                />
              </div>
            </div>
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label>Allow Carry Forward</Label>
                <Switch
                  checked={formData.carryForward}
                  onCheckedChange={(v) => setFormData({ ...formData, carryForward: v })}
                />
              </div>
              {formData.carryForward && (
                <div className="space-y-2 pl-4 border-l-2 border-muted">
                  <Label>Max Carry Forward (days)</Label>
                  <Input
                    type="number"
                    value={formData.maxCarryForward}
                    onChange={(e) => setFormData({ ...formData, maxCarryForward: parseInt(e.target.value) || 0 })}
                  />
                </div>
              )}
              <div className="flex items-center justify-between">
                <Label>Encashable</Label>
                <Switch
                  checked={formData.encashable}
                  onCheckedChange={(v) => setFormData({ ...formData, encashable: v })}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label>Requires Approval</Label>
                <Switch
                  checked={formData.requiresApproval}
                  onCheckedChange={(v) => setFormData({ ...formData, requiresApproval: v })}
                />
              </div>
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingLeaveType ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
