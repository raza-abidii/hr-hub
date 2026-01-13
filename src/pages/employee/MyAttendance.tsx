import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatCard } from "@/components/ui/stat-card";
import { StatusBadge } from "@/components/ui/status-badge";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Download, Clock, UserCheck, UserX, AlertTriangle } from "lucide-react";

type AttendanceStatus = 'present' | 'absent' | 'weekend' | 'holiday' | 'leave';

interface AttendanceRecord {
  date: string;
  day: string;
  clockIn: string;
  clockOut: string;
  totalHours: string;
  status: AttendanceStatus;
  isLate?: boolean;
  isEarlyOut?: boolean;
}

const mockAttendanceData: AttendanceRecord[] = [
  { date: "Jan 7, 2026", day: "Tue", clockIn: "09:02 AM", clockOut: "-", totalHours: "-", status: "present" },
  { date: "Jan 6, 2026", day: "Mon", clockIn: "09:00 AM", clockOut: "06:05 PM", totalHours: "9h 5m", status: "present" },
  { date: "Jan 5, 2026", day: "Sun", clockIn: "-", clockOut: "-", totalHours: "-", status: "weekend" },
  { date: "Jan 4, 2026", day: "Sat", clockIn: "-", clockOut: "-", totalHours: "-", status: "weekend" },
  { date: "Jan 3, 2026", day: "Fri", clockIn: "09:15 AM", clockOut: "05:45 PM", totalHours: "8h 30m", status: "present", isLate: true, isEarlyOut: true },
  { date: "Jan 2, 2026", day: "Thu", clockIn: "08:55 AM", clockOut: "06:10 PM", totalHours: "9h 15m", status: "present" },
  { date: "Jan 1, 2026", day: "Wed", clockIn: "-", clockOut: "-", totalHours: "-", status: "holiday" },
  { date: "Dec 31, 2025", day: "Tue", clockIn: "-", clockOut: "-", totalHours: "-", status: "leave" },
  { date: "Dec 30, 2025", day: "Mon", clockIn: "09:00 AM", clockOut: "06:00 PM", totalHours: "9h 0m", status: "present" },
];

const months = [
  "January 2026", "December 2025", "November 2025", 
  "October 2025", "September 2025", "August 2025"
];

export default function MyAttendance() {
  const [selectedMonth, setSelectedMonth] = useState("January 2026");

  // Calculate summary stats
  const presentDays = mockAttendanceData.filter(d => d.status === 'present').length;
  const absentDays = mockAttendanceData.filter(d => d.status === 'absent').length;
  const lateEarlyDays = mockAttendanceData.filter(d => d.isLate || d.isEarlyOut).length;
  const totalHours = "35h";

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="My Attendance"
        subtitle="View your attendance history"
        action={
          <div className="flex items-center gap-3">
            <Select value={selectedMonth} onValueChange={setSelectedMonth}>
              <SelectTrigger className="w-44">
                <SelectValue placeholder="Select month" />
              </SelectTrigger>
              <SelectContent>
                {months.map((month) => (
                  <SelectItem key={month} value={month}>
                    {month}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button variant="outline" size="icon">
              <Download className="h-4 w-4" />
            </Button>
          </div>
        }
      />

      {/* Summary Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="Days Present"
          value={presentDays}
          icon={UserCheck}
          variant="success"
        />
        <StatCard
          title="Days Absent"
          value={absentDays}
          icon={UserX}
          variant="danger"
        />
        <StatCard
          title="Late/Early"
          value={lateEarlyDays}
          icon={AlertTriangle}
          variant="warning"
        />
        <StatCard
          title="Total Hours"
          value={totalHours}
          icon={Clock}
          variant="info"
        />
      </div>

      {/* Attendance Records */}
      <Card className="hrms-card">
        <CardContent className="p-0">
          <div className="divide-y divide-border">
            {mockAttendanceData.map((record, index) => (
              <div
                key={index}
                className="p-4 hover:bg-muted/30 transition-colors"
              >
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="font-medium">{record.date}</span>
                      <span className="text-sm text-muted-foreground">
                        ({record.day})
                      </span>
                    </div>
                    {record.status !== 'weekend' && record.status !== 'holiday' && record.status !== 'leave' ? (
                      <p className="text-sm text-muted-foreground">
                        Clock In: {record.clockIn} | Clock Out: {record.clockOut}
                      </p>
                    ) : (
                      <p className="text-sm text-muted-foreground">
                        No attendance record
                      </p>
                    )}
                  </div>

                  <div className="flex items-center gap-2 flex-wrap">
                    {record.isLate && <StatusBadge variant="late" />}
                    {record.isEarlyOut && <StatusBadge variant="early" />}
                    <StatusBadge variant={record.status} />
                    {record.totalHours !== '-' && (
                      <span className="text-lg font-semibold text-primary ml-2">
                        {record.totalHours}
                      </span>
                    )}
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
