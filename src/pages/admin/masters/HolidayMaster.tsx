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
import { Plus, Search, Pencil, Trash2, Calendar, Star, MapPin } from "lucide-react";

interface Holiday {
  id: string;
  name: string;
  date: string;
  day: string;
  type: string;
  applicable: string;
}

const initialHolidays: Holiday[] = [
  { id: "1", name: "New Year's Day", date: "2026-01-01", day: "Thu", type: "National", applicable: "All" },
  { id: "2", name: "Republic Day", date: "2026-01-26", day: "Mon", type: "National", applicable: "All" },
  { id: "3", name: "Holi", date: "2026-03-08", day: "Sun", type: "Regional", applicable: "India" },
  { id: "4", name: "Good Friday", date: "2026-04-03", day: "Fri", type: "National", applicable: "All" },
  { id: "5", name: "Independence Day", date: "2026-08-15", day: "Sat", type: "National", applicable: "All" },
  { id: "6", name: "Diwali", date: "2026-10-20", day: "Tue", type: "National", applicable: "All" },
  { id: "7", name: "Christmas", date: "2026-12-25", day: "Fri", type: "National", applicable: "All" },
];

const holidayTypes = ["National", "Regional", "Optional", "Floating"];

export default function HolidayMaster() {
  const [holidays, setHolidays] = useState(initialHolidays);
  const [searchQuery, setSearchQuery] = useState("");
  const [year, setYear] = useState("2026");
  const [showDialog, setShowDialog] = useState(false);
  const [editingHoliday, setEditingHoliday] = useState<Holiday | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    date: "",
    type: "",
    applicable: "All",
  });

  const getDayFromDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", { weekday: "short" });
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
  };

  const filteredHolidays = holidays.filter(
    (h) =>
      h.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      h.type.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = () => {
    if (!formData.name || !formData.date || !formData.type) {
      toast.error("Please fill all required fields");
      return;
    }

    const day = getDayFromDate(formData.date);

    if (editingHoliday) {
      setHolidays((prev) =>
        prev.map((h) =>
          h.id === editingHoliday.id ? { ...h, ...formData, day } : h
        )
      );
      toast.success("Holiday updated successfully!");
    } else {
      setHolidays((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData, day },
      ]);
      toast.success("Holiday added successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (holiday: Holiday) => {
    setEditingHoliday(holiday);
    setFormData({
      name: holiday.name,
      date: holiday.date,
      type: holiday.type,
      applicable: holiday.applicable,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    setHolidays((prev) => prev.filter((h) => h.id !== id));
    toast.success("Holiday deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingHoliday(null);
    setFormData({ name: "", date: "", type: "", applicable: "All" });
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case "National":
        return "bg-primary/10 text-primary";
      case "Regional":
        return "bg-warning/10 text-warning";
      case "Optional":
        return "bg-info/10 text-info";
      case "Floating":
        return "bg-success/10 text-success";
      default:
        return "bg-muted text-muted-foreground";
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Holiday Master"
          subtitle="Manage company holidays and observances"
        />
        <div className="flex gap-2">
          <Select value={year} onValueChange={setYear}>
            <SelectTrigger className="w-28">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="2025">2025</SelectItem>
              <SelectItem value="2026">2026</SelectItem>
              <SelectItem value="2027">2027</SelectItem>
            </SelectContent>
          </Select>
          <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
            <Plus className="h-4 w-4 mr-2" /> Add Holiday
          </Button>
        </div>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search holidays..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredHolidays.map((holiday) => (
          <Card key={holiday.id} className="hrms-card hover:shadow-md transition-shadow">
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Calendar className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold">{holiday.name}</h3>
                  </div>
                  <p className="text-sm text-muted-foreground">
                    {formatDate(holiday.date)} ({holiday.day})
                  </p>
                  <div className="flex flex-wrap gap-2">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getTypeColor(holiday.type)}`}>
                      {holiday.type}
                    </span>
                    <span className="px-2 py-1 rounded-full bg-muted text-muted-foreground text-xs font-medium flex items-center gap-1">
                      <MapPin className="h-3 w-3" />
                      {holiday.applicable}
                    </span>
                  </div>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon" onClick={() => handleEdit(holiday)}>
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-destructive"
                    onClick={() => handleDelete(holiday.id)}
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
              {editingHoliday ? "Edit Holiday" : "Add New Holiday"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Holiday Name *</Label>
              <Input
                placeholder="e.g., Independence Day"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label>Date *</Label>
              <Input
                type="date"
                value={formData.date}
                onChange={(e) => setFormData({ ...formData, date: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label>Type *</Label>
              <Select value={formData.type} onValueChange={(v) => setFormData({ ...formData, type: v })}>
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {holidayTypes.map((t) => (
                    <SelectItem key={t} value={t}>{t}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Applicable To</Label>
              <Input
                placeholder="e.g., All, India, Mumbai Office"
                value={formData.applicable}
                onChange={(e) => setFormData({ ...formData, applicable: e.target.value })}
              />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingHoliday ? "Update" : "Add"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
