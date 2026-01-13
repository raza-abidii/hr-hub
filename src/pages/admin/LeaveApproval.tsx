import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "@/components/ui/status-badge";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/checkbox";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { toast } from "sonner";
import {
  Calendar,
  Clock,
  Check,
  X,
  Search,
  Filter,
  CheckCircle2,
  XCircle,
  MessageSquare,
  ChevronDown,
} from "lucide-react";

interface LeaveRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  department: string;
  leaveType: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason: string;
  status: "pending" | "approved" | "rejected";
  appliedDate: string;
  approvedBy?: string;
  approvedDate?: string;
  comments?: string;
  leaveBalance: number;
}

const mockLeaveRequests: LeaveRequest[] = [
  {
    id: "LR001",
    employeeId: "EMP001",
    employeeName: "Sarah Johnson",
    department: "IT",
    leaveType: "Casual Leave",
    fromDate: "Jan 15, 2026",
    toDate: "Jan 17, 2026",
    days: 3,
    reason: "Family function - sister's wedding",
    status: "pending",
    appliedDate: "Jan 7, 2026",
    leaveBalance: 8,
  },
  {
    id: "LR002",
    employeeId: "EMP002",
    employeeName: "Mike Davis",
    department: "Sales",
    leaveType: "Sick Leave",
    fromDate: "Jan 10, 2026",
    toDate: "Jan 11, 2026",
    days: 2,
    reason: "Medical appointment and recovery",
    status: "pending",
    appliedDate: "Jan 8, 2026",
    leaveBalance: 6,
  },
  {
    id: "LR003",
    employeeId: "EMP003",
    employeeName: "Emily Brown",
    department: "HR",
    leaveType: "Earned Leave",
    fromDate: "Jan 20, 2026",
    toDate: "Jan 25, 2026",
    days: 6,
    reason: "Vacation - planned trip",
    status: "pending",
    appliedDate: "Jan 5, 2026",
    leaveBalance: 12,
  },
  {
    id: "LR004",
    employeeId: "EMP004",
    employeeName: "John Smith",
    department: "Finance",
    leaveType: "Casual Leave",
    fromDate: "Jan 8, 2026",
    toDate: "Jan 8, 2026",
    days: 1,
    reason: "Personal work",
    status: "approved",
    appliedDate: "Jan 6, 2026",
    approvedBy: "Admin",
    approvedDate: "Jan 6, 2026",
    leaveBalance: 10,
  },
  {
    id: "LR005",
    employeeId: "EMP005",
    employeeName: "Lisa Wilson",
    department: "Marketing",
    leaveType: "Sick Leave",
    fromDate: "Jan 3, 2026",
    toDate: "Jan 3, 2026",
    days: 1,
    reason: "Fever",
    status: "rejected",
    appliedDate: "Jan 3, 2026",
    approvedBy: "Admin",
    approvedDate: "Jan 3, 2026",
    comments: "No sick leave balance remaining",
    leaveBalance: 0,
  },
];

export default function LeaveApproval() {
  const [searchQuery, setSearchQuery] = useState("");
  const [leaveRequests, setLeaveRequests] = useState(mockLeaveRequests);
  const [selectedRequests, setSelectedRequests] = useState<string[]>([]);
  const [showApproveDialog, setShowApproveDialog] = useState(false);
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [currentRequest, setCurrentRequest] = useState<LeaveRequest | null>(null);
  const [comments, setComments] = useState("");

  const pendingRequests = leaveRequests.filter((r) => r.status === "pending");
  const approvedRequests = leaveRequests.filter((r) => r.status === "approved");
  const rejectedRequests = leaveRequests.filter((r) => r.status === "rejected");

  const filteredPending = pendingRequests.filter(
    (r) =>
      r.employeeName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.department.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.leaveType.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSelectRequest = (id: string) => {
    setSelectedRequests((prev) =>
      prev.includes(id) ? prev.filter((r) => r !== id) : [...prev, id]
    );
  };

  const handleSelectAll = () => {
    if (selectedRequests.length === filteredPending.length) {
      setSelectedRequests([]);
    } else {
      setSelectedRequests(filteredPending.map((r) => r.id));
    }
  };

  const handleApprove = (request?: LeaveRequest) => {
    if (request) {
      setCurrentRequest(request);
    }
    setShowApproveDialog(true);
  };

  const handleReject = (request?: LeaveRequest) => {
    if (request) {
      setCurrentRequest(request);
    }
    setShowRejectDialog(true);
  };

  const confirmApprove = () => {
    const idsToApprove = currentRequest ? [currentRequest.id] : selectedRequests;
    setLeaveRequests((prev) =>
      prev.map((r) =>
        idsToApprove.includes(r.id)
          ? {
              ...r,
              status: "approved" as const,
              approvedBy: "Admin",
              approvedDate: new Date().toLocaleDateString("en-US", {
                month: "short",
                day: "numeric",
                year: "numeric",
              }),
              comments: comments || undefined,
            }
          : r
      )
    );
    toast.success(
      `${idsToApprove.length} leave request(s) approved successfully!`
    );
    setShowApproveDialog(false);
    setCurrentRequest(null);
    setSelectedRequests([]);
    setComments("");
  };

  const confirmReject = () => {
    if (!comments.trim()) {
      toast.error("Please provide a reason for rejection");
      return;
    }
    const idsToReject = currentRequest ? [currentRequest.id] : selectedRequests;
    setLeaveRequests((prev) =>
      prev.map((r) =>
        idsToReject.includes(r.id)
          ? {
              ...r,
              status: "rejected" as const,
              approvedBy: "Admin",
              approvedDate: new Date().toLocaleDateString("en-US", {
                month: "short",
                day: "numeric",
                year: "numeric",
              }),
              comments,
            }
          : r
      )
    );
    toast.info(`${idsToReject.length} leave request(s) rejected`);
    setShowRejectDialog(false);
    setCurrentRequest(null);
    setSelectedRequests([]);
    setComments("");
  };

  const LeaveRequestCard = ({
    request,
    showActions = true,
  }: {
    request: LeaveRequest;
    showActions?: boolean;
  }) => (
    <div className="p-4 rounded-lg border border-border bg-card hover:shadow-md transition-shadow">
      <div className="flex items-start gap-4">
        {showActions && request.status === "pending" && (
          <Checkbox
            checked={selectedRequests.includes(request.id)}
            onCheckedChange={() => handleSelectRequest(request.id)}
            className="mt-1"
          />
        )}
        <Avatar className="h-10 w-10">
          <AvatarFallback className="bg-primary/10 text-primary font-semibold">
            {request.employeeName
              .split(" ")
              .map((n) => n[0])
              .join("")}
          </AvatarFallback>
        </Avatar>
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div>
              <h4 className="font-semibold text-sm">{request.employeeName}</h4>
              <p className="text-xs text-muted-foreground">
                {request.department} • {request.employeeId}
              </p>
            </div>
            <StatusBadge variant={request.status} />
          </div>

          <div className="mt-3 space-y-2">
            <div className="flex items-center gap-4 text-sm">
              <span className="font-medium text-primary">{request.leaveType}</span>
              <span className="text-muted-foreground">
                {request.fromDate} - {request.toDate}
              </span>
              <span className="px-2 py-0.5 rounded-full bg-muted text-xs font-medium">
                {request.days} day{request.days > 1 ? "s" : ""}
              </span>
            </div>
            <p className="text-sm text-muted-foreground">{request.reason}</p>
            <div className="flex items-center gap-4 text-xs text-muted-foreground">
              <span>Applied: {request.appliedDate}</span>
              <span>Balance: {request.leaveBalance} days</span>
            </div>
            {request.comments && (
              <div className="flex items-start gap-2 p-2 rounded bg-muted/50 text-xs">
                <MessageSquare className="h-3 w-3 mt-0.5 text-muted-foreground" />
                <span>{request.comments}</span>
              </div>
            )}
          </div>

          {showActions && request.status === "pending" && (
            <div className="flex gap-2 mt-3">
              <Button
                size="sm"
                className="h-8 bg-success hover:bg-success/90 text-success-foreground"
                onClick={() => handleApprove(request)}
              >
                <Check className="h-3 w-3 mr-1" /> Approve
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="h-8 border-destructive text-destructive hover:bg-destructive/10"
                onClick={() => handleReject(request)}
              >
                <X className="h-3 w-3 mr-1" /> Reject
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Leave Approval"
        subtitle="Review and manage employee leave requests"
      />

      {/* Stats Cards */}
      <div className="grid grid-cols-3 gap-4">
        <Card className="hrms-card">
          <CardContent className="p-4 flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-warning/10 flex items-center justify-center">
              <Clock className="h-5 w-5 text-warning" />
            </div>
            <div>
              <p className="text-2xl font-bold">{pendingRequests.length}</p>
              <p className="text-xs text-muted-foreground">Pending</p>
            </div>
          </CardContent>
        </Card>
        <Card className="hrms-card">
          <CardContent className="p-4 flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-success/10 flex items-center justify-center">
              <CheckCircle2 className="h-5 w-5 text-success" />
            </div>
            <div>
              <p className="text-2xl font-bold">{approvedRequests.length}</p>
              <p className="text-xs text-muted-foreground">Approved</p>
            </div>
          </CardContent>
        </Card>
        <Card className="hrms-card">
          <CardContent className="p-4 flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-destructive/10 flex items-center justify-center">
              <XCircle className="h-5 w-5 text-destructive" />
            </div>
            <div>
              <p className="text-2xl font-bold">{rejectedRequests.length}</p>
              <p className="text-xs text-muted-foreground">Rejected</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Search and Bulk Actions */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search by name, department, or leave type..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10"
          />
        </div>
        {selectedRequests.length > 0 && (
          <div className="flex gap-2">
            <Button
              className="bg-success hover:bg-success/90 text-success-foreground"
              onClick={() => handleApprove()}
            >
              <Check className="h-4 w-4 mr-1" /> Approve ({selectedRequests.length})
            </Button>
            <Button
              variant="outline"
              className="border-destructive text-destructive hover:bg-destructive/10"
              onClick={() => handleReject()}
            >
              <X className="h-4 w-4 mr-1" /> Reject ({selectedRequests.length})
            </Button>
          </div>
        )}
      </div>

      {/* Tabs */}
      <Tabs defaultValue="pending" className="space-y-4">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="pending" className="gap-2">
            Pending
            {pendingRequests.length > 0 && (
              <span className="px-2 py-0.5 rounded-full bg-warning text-warning-foreground text-xs">
                {pendingRequests.length}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="approved">Approved</TabsTrigger>
          <TabsTrigger value="rejected">Rejected</TabsTrigger>
        </TabsList>

        <TabsContent value="pending" className="space-y-3">
          {filteredPending.length > 0 && (
            <div className="flex items-center gap-2 pb-2">
              <Checkbox
                checked={selectedRequests.length === filteredPending.length}
                onCheckedChange={handleSelectAll}
              />
              <span className="text-sm text-muted-foreground">Select All</span>
            </div>
          )}
          {filteredPending.length > 0 ? (
            filteredPending.map((request) => (
              <LeaveRequestCard key={request.id} request={request} />
            ))
          ) : (
            <div className="text-center py-12 text-muted-foreground">
              <Calendar className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>No pending leave requests</p>
            </div>
          )}
        </TabsContent>

        <TabsContent value="approved" className="space-y-3">
          {approvedRequests.length > 0 ? (
            approvedRequests.map((request) => (
              <LeaveRequestCard key={request.id} request={request} showActions={false} />
            ))
          ) : (
            <div className="text-center py-12 text-muted-foreground">
              <CheckCircle2 className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>No approved leave requests</p>
            </div>
          )}
        </TabsContent>

        <TabsContent value="rejected" className="space-y-3">
          {rejectedRequests.length > 0 ? (
            rejectedRequests.map((request) => (
              <LeaveRequestCard key={request.id} request={request} showActions={false} />
            ))
          ) : (
            <div className="text-center py-12 text-muted-foreground">
              <XCircle className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>No rejected leave requests</p>
            </div>
          )}
        </TabsContent>
      </Tabs>

      {/* Approve Dialog */}
      <Dialog open={showApproveDialog} onOpenChange={setShowApproveDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-success">
              <CheckCircle2 className="h-5 w-5" />
              Approve Leave Request
            </DialogTitle>
            <DialogDescription>
              {currentRequest
                ? `Approve ${currentRequest.employeeName}'s ${currentRequest.leaveType} for ${currentRequest.days} day(s)?`
                : `Approve ${selectedRequests.length} leave request(s)?`}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div>
              <label className="text-sm font-medium">Comments (Optional)</label>
              <Textarea
                placeholder="Add any comments or notes..."
                value={comments}
                onChange={(e) => setComments(e.target.value)}
                className="mt-1"
              />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={() => setShowApproveDialog(false)}>
              Cancel
            </Button>
            <Button className="flex-1 bg-success hover:bg-success/90 text-success-foreground" onClick={confirmApprove}>
              Approve
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={showRejectDialog} onOpenChange={setShowRejectDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-destructive">
              <XCircle className="h-5 w-5" />
              Reject Leave Request
            </DialogTitle>
            <DialogDescription>
              {currentRequest
                ? `Reject ${currentRequest.employeeName}'s ${currentRequest.leaveType}?`
                : `Reject ${selectedRequests.length} leave request(s)?`}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div>
              <label className="text-sm font-medium">Reason for Rejection *</label>
              <Textarea
                placeholder="Please provide a reason for rejection..."
                value={comments}
                onChange={(e) => setComments(e.target.value)}
                className="mt-1"
                required
              />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={() => setShowRejectDialog(false)}>
              Cancel
            </Button>
            <Button variant="destructive" className="flex-1" onClick={confirmReject}>
              Reject
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
