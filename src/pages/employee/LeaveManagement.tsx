import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatusBadge } from "@/components/ui/status-badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
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
  DialogTrigger,
} from "@/components/ui/dialog";
import { Calendar, Plus, FileText, Upload, CalendarDays } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { format } from "date-fns";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Calendar as CalendarComponent } from "@/components/ui/calendar";
import { cn } from "@/lib/utils";

interface LeaveRequest {
  id: string;
  type: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason: string;
  status: 'pending' | 'approved' | 'rejected';
  appliedDate: string;
  approverComment?: string;
}

const mockLeaveRequests: LeaveRequest[] = [
  {
    id: "1",
    type: "Casual Leave",
    fromDate: "Jan 15, 2026",
    toDate: "Jan 16, 2026",
    days: 2,
    reason: "Personal work",
    status: "pending",
    appliedDate: "Jan 7, 2026",
  },
  {
    id: "2",
    type: "Sick Leave",
    fromDate: "Dec 20, 2025",
    toDate: "Dec 20, 2025",
    days: 1,
    reason: "Not feeling well",
    status: "approved",
    appliedDate: "Dec 19, 2025",
    approverComment: "Get well soon!",
  },
  {
    id: "3",
    type: "Earned Leave",
    fromDate: "Nov 10, 2025",
    toDate: "Nov 15, 2025",
    days: 5,
    reason: "Family vacation",
    status: "approved",
    appliedDate: "Oct 25, 2025",
  },
  {
    id: "4",
    type: "Casual Leave",
    fromDate: "Oct 5, 2025",
    toDate: "Oct 5, 2025",
    days: 1,
    reason: "Personal emergency",
    status: "rejected",
    appliedDate: "Oct 4, 2025",
    approverComment: "Team has critical deadline. Please reschedule.",
  },
];

const leaveBalance = [
  { type: "Casual Leave", code: "CL", used: 4, total: 12, color: "bg-blue-500" },
  { type: "Sick Leave", code: "SL", used: 2, total: 10, color: "bg-purple-500" },
  { type: "Earned Leave", code: "EL", used: 6, total: 18, color: "bg-primary" },
  { type: "Compensatory Off", code: "CO", used: 0, total: 2, color: "bg-info" },
];

const leaveTypes = [
  { value: "casual", label: "Casual Leave", remaining: 8 },
  { value: "sick", label: "Sick Leave", remaining: 8 },
  { value: "earned", label: "Earned Leave", remaining: 12 },
  { value: "comp", label: "Compensatory Off", remaining: 2 },
];

export default function LeaveManagement() {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [fromDate, setFromDate] = useState<Date>();
  const [toDate, setToDate] = useState<Date>();
  const [formData, setFormData] = useState({
    leaveType: "",
    reason: "",
  });
  const { toast } = useToast();

  const calculateDays = () => {
    if (!fromDate || !toDate) return 0;
    const diffTime = Math.abs(toDate.getTime() - fromDate.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    return diffDays;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.leaveType) {
      toast({ title: "Error", description: "Please select leave type", variant: "destructive" });
      return;
    }
    if (!fromDate || !toDate) {
      toast({ title: "Error", description: "Please select dates", variant: "destructive" });
      return;
    }
    if (fromDate > toDate) {
      toast({ title: "Error", description: "From date cannot be after To date", variant: "destructive" });
      return;
    }
    if (!formData.reason.trim()) {
      toast({ title: "Error", description: "Please provide a reason for leave", variant: "destructive" });
      return;
    }

    const selectedLeave = leaveTypes.find(l => l.value === formData.leaveType);
    const days = calculateDays();
    if (selectedLeave && days > selectedLeave.remaining) {
      toast({ 
        title: "Insufficient Balance", 
        description: `Available: ${selectedLeave.remaining} days. Requested: ${days} days`, 
        variant: "destructive" 
      });
      return;
    }

    toast({
      title: "Leave Applied!",
      description: "Your leave request has been submitted for approval.",
    });

    setIsDialogOpen(false);
    setFormData({ leaveType: "", reason: "" });
    setFromDate(undefined);
    setToDate(undefined);
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Leave Management"
        subtitle="Apply for and track your leaves"
        action={
          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogTrigger asChild>
              <Button className="gradient-primary">
                <Plus className="h-4 w-4 mr-2" />
                Apply Leave
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-lg">
              <DialogHeader>
                <DialogTitle className="font-display flex items-center gap-2">
                  <CalendarDays className="h-5 w-5 text-primary" />
                  Apply for Leave
                </DialogTitle>
              </DialogHeader>
              <form onSubmit={handleSubmit} className="space-y-4 mt-4">
                <div className="space-y-2">
                  <Label>Leave Type *</Label>
                  <Select
                    value={formData.leaveType}
                    onValueChange={(value) =>
                      setFormData({ ...formData, leaveType: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select leave type" />
                    </SelectTrigger>
                    <SelectContent className="bg-popover">
                      {leaveTypes.map((type) => (
                        <SelectItem key={type.value} value={type.value}>
                          <span className="flex items-center justify-between gap-4">
                            {type.label}
                            <span className="text-xs text-muted-foreground">
                              ({type.remaining} remaining)
                            </span>
                          </span>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>From Date *</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          className={cn(
                            "w-full justify-start text-left font-normal",
                            !fromDate && "text-muted-foreground"
                          )}
                        >
                          <Calendar className="mr-2 h-4 w-4" />
                          {fromDate ? format(fromDate, "PPP") : "Select date"}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0 bg-popover" align="start">
                        <CalendarComponent
                          mode="single"
                          selected={fromDate}
                          onSelect={setFromDate}
                          initialFocus
                          className="p-3 pointer-events-auto"
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                  <div className="space-y-2">
                    <Label>To Date *</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          className={cn(
                            "w-full justify-start text-left font-normal",
                            !toDate && "text-muted-foreground"
                          )}
                        >
                          <Calendar className="mr-2 h-4 w-4" />
                          {toDate ? format(toDate, "PPP") : "Select date"}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0 bg-popover" align="start">
                        <CalendarComponent
                          mode="single"
                          selected={toDate}
                          onSelect={setToDate}
                          initialFocus
                          className="p-3 pointer-events-auto"
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                </div>

                {fromDate && toDate && fromDate <= toDate && (
                  <div className="p-3 rounded-lg bg-accent/50 border border-accent">
                    <p className="text-sm font-medium text-accent-foreground">
                      Duration: {calculateDays()} day{calculateDays() > 1 ? 's' : ''}
                    </p>
                  </div>
                )}

                <div className="space-y-2">
                  <Label>Reason *</Label>
                  <Textarea
                    placeholder="Enter reason for leave"
                    value={formData.reason}
                    onChange={(e) =>
                      setFormData({ ...formData, reason: e.target.value })
                    }
                    rows={3}
                  />
                </div>

                <div className="space-y-2">
                  <Label>Attachment (Optional)</Label>
                  <div className="border-2 border-dashed border-border rounded-lg p-4 text-center hover:border-primary/50 transition-colors cursor-pointer">
                    <Upload className="h-6 w-6 text-muted-foreground mx-auto mb-2" />
                    <p className="text-sm text-muted-foreground">
                      Click to upload or drag and drop
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      PDF, JPG, PNG (max 5MB)
                    </p>
                  </div>
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
                    Submit Request
                  </Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        }
      />

      {/* Leave Balance Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {leaveBalance.map((leave) => {
          const remaining = leave.total - leave.used;
          const percentage = (remaining / leave.total) * 100;
          
          return (
            <Card key={leave.code} className="hrms-card overflow-hidden">
              <div className={cn("h-1.5", leave.color)} />
              <CardContent className="p-4">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">
                      {leave.type}
                    </p>
                    <p className="text-3xl font-display font-bold text-foreground mt-1">
                      {remaining}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      of {leave.total} days remaining
                    </p>
                  </div>
                  <span className={cn(
                    "px-2 py-1 text-xs font-bold rounded text-white",
                    leave.color
                  )}>
                    {leave.code}
                  </span>
                </div>
                <div className="space-y-1">
                  <div className="flex justify-between text-xs text-muted-foreground">
                    <span>Used: {leave.used}</span>
                    <span>{Math.round(percentage)}%</span>
                  </div>
                  <div className="h-2 bg-muted rounded-full overflow-hidden">
                    <div
                      className={cn("h-full rounded-full transition-all", leave.color)}
                      style={{ width: `${percentage}%` }}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Leave History */}
      <Card className="hrms-card">
        <CardHeader>
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <FileText className="h-5 w-5 text-primary" />
            Leave History
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <div className="divide-y divide-border">
            {mockLeaveRequests.map((request) => (
              <div
                key={request.id}
                className="p-4 hover:bg-muted/30 transition-colors"
              >
                <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="font-semibold">{request.type}</span>
                      <StatusBadge variant={request.status} />
                      <span className="px-2 py-0.5 bg-secondary text-secondary-foreground text-xs font-medium rounded">
                        {request.days} day{request.days > 1 ? 's' : ''}
                      </span>
                    </div>
                    <p className="text-sm text-muted-foreground mb-1">
                      📅 {request.fromDate} - {request.toDate}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      Reason: {request.reason}
                    </p>
                    {request.approverComment && (
                      <div className="mt-2 p-2 rounded bg-muted/50">
                        <p className="text-sm text-primary italic">
                          💬 "{request.approverComment}"
                        </p>
                      </div>
                    )}
                  </div>
                  <div className="text-right">
                    <p className="text-xs text-muted-foreground">
                      Applied on {request.appliedDate}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
