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
} from "@/components/ui/dialog";
import { toast } from "sonner";
import { Plus, Search, Pencil, Trash2, Clock, Sun, Moon, Sunset, Users } from "lucide-react";

interface Shift {
  id: string;
  name: string;
  code: string;
  startTime: string;
  endTime: string;
  breakDuration: number;
  gracePeriod: number;
  weekOff: string[];
  employeeCount: number;
}

const initialShifts: Shift[] = [
  { id: "1", name: "General Shift", code: "GEN", startTime: "09:00", endTime: "18:00", breakDuration: 60, gracePeriod: 15, weekOff: ["Sat", "Sun"], employeeCount: 35 },
  { id: "2", name: "Morning Shift", code: "MOR", startTime: "06:00", endTime: "15:00", breakDuration: 30, gracePeriod: 10, weekOff: ["Sun"], employeeCount: 8 },
  { id: "3", name: "Evening Shift", code: "EVE", startTime: "15:00", endTime: "00:00", breakDuration: 30, gracePeriod: 10, weekOff: ["Mon", "Tue"], employeeCount: 5 },
  { id: "4", name: "Night Shift", code: "NGT", startTime: "22:00", endTime: "07:00", breakDuration: 60, gracePeriod: 15, weekOff: ["Sat", "Sun"], employeeCount: 2 },
];

const weekDays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

export default function ShiftMaster() {
  const [shifts, setShifts] = useState(initialShifts);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingShift, setEditingShift] = useState<Shift | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    startTime: "",
    endTime: "",
    breakDuration: 60,
    gracePeriod: 15,
    weekOff: ["Sat", "Sun"] as string[],
  });

  const filteredShifts = shifts.filter(
    (s) =>
      s.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      s.code.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const formatTime = (time: string) => {
    const [hours, minutes] = time.split(":");
    const h = parseInt(hours);
    const ampm = h >= 12 ? "PM" : "AM";
    const h12 = h % 12 || 12;
    return `${h12}:${minutes} ${ampm}`;
  };

  const getShiftIcon = (code: string) => {
    switch (code) {
      case "MOR":
        return <Sun className="h-5 w-5 text-warning" />;
      case "EVE":
        return <Sunset className="h-5 w-5 text-orange-500" />;
      case "NGT":
        return <Moon className="h-5 w-5 text-info" />;
      default:
        return <Clock className="h-5 w-5 text-primary" />;
    }
  };

  const handleSubmit = () => {
    if (!formData.name || !formData.code || !formData.startTime || !formData.endTime) {
      toast.error("Please fill all required fields");
      return;
    }

    if (editingShift) {
      setShifts((prev) =>
        prev.map((s) =>
          s.id === editingShift.id ? { ...s, ...formData } : s
        )
      );
      toast.success("Shift updated successfully!");
    } else {
      setShifts((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData, employeeCount: 0 },
      ]);
      toast.success("Shift created successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (shift: Shift) => {
    setEditingShift(shift);
    setFormData({
      name: shift.name,
      code: shift.code,
      startTime: shift.startTime,
      endTime: shift.endTime,
      breakDuration: shift.breakDuration,
      gracePeriod: shift.gracePeriod,
      weekOff: shift.weekOff,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    const shift = shifts.find((s) => s.id === id);
    if (shift?.employeeCount && shift.employeeCount > 0) {
      toast.error("Cannot delete shift with assigned employees");
      return;
    }
    setShifts((prev) => prev.filter((s) => s.id !== id));
    toast.success("Shift deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingShift(null);
    setFormData({
      name: "",
      code: "",
      startTime: "",
      endTime: "",
      breakDuration: 60,
      gracePeriod: 15,
      weekOff: ["Sat", "Sun"],
    });
  };

  const toggleWeekOff = (day: string) => {
    setFormData((prev) => ({
      ...prev,
      weekOff: prev.weekOff.includes(day)
        ? prev.weekOff.filter((d) => d !== day)
        : [...prev.weekOff, day],
    }));
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Shift Master"
          subtitle="Define work shifts and schedules"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Shift
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search shifts..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {filteredShifts.map((shift) => (
          <Card key={shift.id} className="hrms-card hover:shadow-md transition-shadow">
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    {getShiftIcon(shift.code)}
                    <div>
                      <h3 className="font-semibold">{shift.name}</h3>
                      <span className="text-xs text-muted-foreground">{shift.code}</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-4 text-sm">
                    <span className="font-medium text-primary">
                      {formatTime(shift.startTime)} - {formatTime(shift.endTime)}
                    </span>
                  </div>
                  <div className="flex flex-wrap gap-2 text-xs">
                    <span className="px-2 py-1 rounded-full bg-muted text-muted-foreground">
                      Break: {shift.breakDuration} min
                    </span>
                    <span className="px-2 py-1 rounded-full bg-muted text-muted-foreground">
                      Grace: {shift.gracePeriod} min
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground">Week Off:</span>
                    <div className="flex gap-1">
                      {shift.weekOff.map((day) => (
                        <span key={day} className="px-2 py-0.5 rounded bg-warning/10 text-warning text-xs font-medium">
                          {day}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="flex items-center gap-1 text-sm text-muted-foreground">
                    <Users className="h-4 w-4" />
                    <span>{shift.employeeCount} employees</span>
                  </div>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon" onClick={() => handleEdit(shift)}>
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-destructive"
                    onClick={() => handleDelete(shift.id)}
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
              {editingShift ? "Edit Shift" : "Add New Shift"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Shift Name *</Label>
                <Input
                  placeholder="e.g., General Shift"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Code *</Label>
                <Input
                  placeholder="e.g., GEN"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  maxLength={4}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Start Time *</Label>
                <Input
                  type="time"
                  value={formData.startTime}
                  onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>End Time *</Label>
                <Input
                  type="time"
                  value={formData.endTime}
                  onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Break (min)</Label>
                <Input
                  type="number"
                  value={formData.breakDuration}
                  onChange={(e) => setFormData({ ...formData, breakDuration: parseInt(e.target.value) || 0 })}
                />
              </div>
              <div className="space-y-2">
                <Label>Grace (min)</Label>
                <Input
                  type="number"
                  value={formData.gracePeriod}
                  onChange={(e) => setFormData({ ...formData, gracePeriod: parseInt(e.target.value) || 0 })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Week Off Days</Label>
              <div className="flex flex-wrap gap-2">
                {weekDays.map((day) => (
                  <Button
                    key={day}
                    type="button"
                    variant={formData.weekOff.includes(day) ? "default" : "outline"}
                    size="sm"
                    onClick={() => toggleWeekOff(day)}
                    className={formData.weekOff.includes(day) ? "gradient-primary text-primary-foreground" : ""}
                  >
                    {day}
                  </Button>
                ))}
              </div>
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingShift ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
