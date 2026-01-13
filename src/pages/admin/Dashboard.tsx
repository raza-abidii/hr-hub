import { useState, useEffect } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatCard } from "@/components/ui/stat-card";
import { StatusBadge } from "@/components/ui/status-badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import {
  Users,
  UserCheck,
  UserX,
  Clock,
  Calendar,
  TrendingUp,
  AlertCircle,
  LogIn,
  LogOut,
  Pause,
  Play,
  Coffee,
  PieChart,
  BarChart3,
} from "lucide-react";
import { Link } from "react-router-dom";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { PieChart as RechartsPie, Pie, Cell, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Legend, LineChart, Line, Tooltip } from "recharts";

const recentLeaveRequests = [
  { id: 1, name: "John Doe", type: "Casual Leave", dates: "Jan 8-9", status: "pending" as const },
  { id: 2, name: "Jane Smith", type: "Sick Leave", dates: "Jan 7", status: "approved" as const },
  { id: 3, name: "Mike Johnson", type: "Earned Leave", dates: "Jan 10-15", status: "pending" as const },
];

const todayAttendance = [
  { id: 1, name: "Sarah Wilson", department: "IT", time: "09:02 AM", status: "present" as const },
  { id: 2, name: "Tom Brown", department: "HR", time: "09:15 AM", status: "late" as const },
  { id: 3, name: "Emily Davis", department: "Finance", time: "-", status: "absent" as const },
];

// Today's attendance distribution data
const todayDistribution = [
  { name: "Present", value: 142, color: "hsl(var(--success))" },
  { name: "Absent", value: 8, color: "hsl(var(--destructive))" },
  { name: "Late", value: 4, color: "hsl(var(--warning))" },
  { name: "On Leave", value: 2, color: "hsl(var(--primary))" },
];

// Weekly attendance data
const weeklyAttendance = [
  { day: "Mon", present: 148, absent: 5, late: 3 },
  { day: "Tue", present: 145, absent: 8, late: 3 },
  { day: "Wed", present: 142, absent: 10, late: 4 },
  { day: "Thu", present: 150, absent: 4, late: 2 },
  { day: "Fri", present: 140, absent: 12, late: 4 },
  { day: "Sat", present: 45, absent: 2, late: 1 },
  { day: "Sun", present: 0, absent: 0, late: 0 },
];

const pieChartConfig = {
  present: { label: "Present", color: "hsl(var(--success))" },
  absent: { label: "Absent", color: "hsl(var(--destructive))" },
  late: { label: "Late", color: "hsl(var(--warning))" },
  onLeave: { label: "On Leave", color: "hsl(var(--primary))" },
};

const barChartConfig = {
  present: { label: "Present", color: "hsl(var(--success))" },
  absent: { label: "Absent", color: "hsl(var(--destructive))" },
  late: { label: "Late", color: "hsl(var(--warning))" },
};

// Monthly attendance trend data
const monthlyAttendance = [
  { month: "Jan", attendance: 94 },
  { month: "Feb", attendance: 92 },
  { month: "Mar", attendance: 96 },
  { month: "Apr", attendance: 93 },
  { month: "May", attendance: 95 },
  { month: "Jun", attendance: 91 },
  { month: "Jul", attendance: 97 },
  { month: "Aug", attendance: 94 },
  { month: "Sep", attendance: 96 },
  { month: "Oct", attendance: 95 },
  { month: "Nov", attendance: 93 },
  { month: "Dec", attendance: 94 },
];

export default function AdminDashboard() {
  const [clockedIn, setClockedIn] = useState(false);
  const [onBreak, setOnBreak] = useState(false);
  const [clockInTime, setClockInTime] = useState<Date | null>(null);
  const [elapsedTime, setElapsedTime] = useState(0);
  const [breakTime, setBreakTime] = useState(0);
  const [totalBreakTime, setTotalBreakTime] = useState(0);

  // Update elapsed time every second
  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (clockedIn && clockInTime) {
      interval = setInterval(() => {
        const now = new Date();
        const totalElapsed = Math.floor((now.getTime() - clockInTime.getTime()) / 1000);
        setElapsedTime(totalElapsed);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [clockedIn, clockInTime]);

  // Track break time
  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (onBreak) {
      interval = setInterval(() => {
        setBreakTime(prev => prev + 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [onBreak]);

  const handleClockIn = () => {
    setClockedIn(true);
    setClockInTime(new Date());
    toast.success("Clocked in successfully!");
  };

  const handleClockOut = () => {
    setClockedIn(false);
    setOnBreak(false);
    setClockInTime(null);
    setElapsedTime(0);
    setBreakTime(0);
    setTotalBreakTime(0);
    toast.success("Clocked out successfully!");
  };

  const toggleBreak = () => {
    if (onBreak) {
      setTotalBreakTime(prev => prev + breakTime);
      setBreakTime(0);
      toast.info("Break ended");
    } else {
      toast.info("Break started");
    }
    setOnBreak(!onBreak);
  };

  const formatTime = (seconds: number) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hrs.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const formatTimeShort = (seconds: number) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    if (hrs > 0) return `${hrs}h ${mins}m`;
    return `${mins}m`;
  };

  const allBreakTime = totalBreakTime + (onBreak ? breakTime : 0);

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Dashboard"
        subtitle="Welcome back! Here's what's happening today."
      />

      {/* Minimalist Clock-In Card */}
      <Card className={cn(
        "hrms-card overflow-hidden transition-all",
        clockedIn && !onBreak && "border-primary/50",
        onBreak && "border-warning/50"
      )}>
        <CardContent className="p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              {/* Status Indicator */}
              <div className={cn(
                "w-12 h-12 rounded-xl flex items-center justify-center transition-colors",
                clockedIn 
                  ? onBreak 
                    ? "bg-warning/10" 
                    : "bg-primary/10"
                  : "bg-muted"
              )}>
                {clockedIn ? (
                  onBreak ? (
                    <Coffee className="h-6 w-6 text-warning" />
                  ) : (
                    <Clock className={cn("h-6 w-6 text-primary", !onBreak && "animate-pulse")} />
                  )
                ) : (
                  <Clock className="h-6 w-6 text-muted-foreground" />
                )}
              </div>

              {/* Time Info */}
              <div>
                <div className="flex items-center gap-2">
                  <span className={cn(
                    "text-xs font-medium px-2 py-0.5 rounded-full",
                    clockedIn 
                      ? onBreak 
                        ? "bg-warning/20 text-warning-foreground" 
                        : "bg-primary/20 text-primary"
                      : "bg-muted text-muted-foreground"
                  )}>
                    {clockedIn ? (onBreak ? "On Break" : "Working") : "Not Clocked In"}
                  </span>
                </div>
                {clockedIn ? (
                  <div className="mt-1">
                    <span className="text-2xl font-mono font-bold">{formatTime(elapsedTime)}</span>
                    <span className="text-sm text-muted-foreground ml-2">
                      since {clockInTime?.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground mt-1">
                    Click to start your day
                  </p>
                )}
                {allBreakTime > 0 && (
                  <p className="text-xs text-muted-foreground mt-0.5">
                    Break: {formatTimeShort(allBreakTime)}
                  </p>
                )}
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex items-center gap-2">
              {clockedIn ? (
                <>
                  <Button
                    onClick={toggleBreak}
                    variant="outline"
                    size="sm"
                    className={cn(
                      "gap-1.5",
                      onBreak 
                        ? "border-primary text-primary hover:bg-primary hover:text-primary-foreground" 
                        : "border-warning text-warning hover:bg-warning hover:text-warning-foreground"
                    )}
                  >
                    {onBreak ? <Play className="h-4 w-4" /> : <Pause className="h-4 w-4" />}
                    {onBreak ? "Resume" : "Break"}
                  </Button>
                  <Button
                    onClick={handleClockOut}
                    variant="outline"
                    size="sm"
                    className="gap-1.5 border-destructive text-destructive hover:bg-destructive hover:text-destructive-foreground"
                  >
                    <LogOut className="h-4 w-4" />
                    Clock Out
                  </Button>
                </>
              ) : (
                <Button
                  onClick={handleClockIn}
                  size="sm"
                  className="gap-1.5 gradient-primary text-primary-foreground"
                >
                  <LogIn className="h-4 w-4" />
                  Clock In
                </Button>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="Total Employees"
          value="156"
          icon={Users}
          trend={{ value: 5.2, isPositive: true }}
          variant="default"
        />
        <StatCard
          title="Present Today"
          value="142"
          icon={UserCheck}
          variant="success"
        />
        <StatCard
          title="Absent Today"
          value="8"
          icon={UserX}
          variant="danger"
        />
        <StatCard
          title="Pending Leaves"
          value="12"
          icon={Clock}
          variant="warning"
        />
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Today's Attendance Distribution - Pie Chart */}
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <PieChart className="h-5 w-5 text-primary" />
              Today's Attendance Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ChartContainer config={pieChartConfig} className="h-[250px] w-full">
              <RechartsPie>
                <Pie
                  data={todayDistribution}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={90}
                  paddingAngle={4}
                  dataKey="value"
                  nameKey="name"
                  label={({ name, value }) => `${name}: ${value}`}
                  labelLine={false}
                >
                  {todayDistribution.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <ChartTooltip content={<ChartTooltipContent />} />
              </RechartsPie>
            </ChartContainer>
            <div className="flex flex-wrap justify-center gap-4 mt-4">
              {todayDistribution.map((item) => (
                <div key={item.name} className="flex items-center gap-2">
                  <div
                    className="w-3 h-3 rounded-full"
                    style={{ backgroundColor: item.color }}
                  />
                  <span className="text-sm text-muted-foreground">
                    {item.name}: {item.value}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Weekly Attendance Distribution - Bar Chart */}
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <BarChart3 className="h-5 w-5 text-primary" />
              Weekly Attendance Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ChartContainer config={barChartConfig} className="h-[280px] w-full">
              <BarChart data={weeklyAttendance}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis
                  dataKey="day"
                  tick={{ fontSize: 12 }}
                  tickLine={false}
                  axisLine={false}
                />
                <YAxis
                  tick={{ fontSize: 12 }}
                  tickLine={false}
                  axisLine={false}
                />
                <ChartTooltip content={<ChartTooltipContent />} />
                <Legend />
                <Bar
                  dataKey="present"
                  name="Present"
                  fill="hsl(var(--success))"
                  radius={[4, 4, 0, 0]}
                />
                <Bar
                  dataKey="absent"
                  name="Absent"
                  fill="hsl(var(--destructive))"
                  radius={[4, 4, 0, 0]}
                />
                <Bar
                  dataKey="late"
                  name="Late"
                  fill="hsl(var(--warning))"
                  radius={[4, 4, 0, 0]}
                />
              </BarChart>
            </ChartContainer>
          </CardContent>
        </Card>
      </div>

      {/* Monthly Attendance Trend */}
      <Card className="hrms-card">
        <CardHeader className="pb-2">
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <TrendingUp className="h-5 w-5 text-primary" />
            Monthly Attendance Trend
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={monthlyAttendance}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis dataKey="month" className="text-xs" />
                <YAxis domain={[80, 100]} className="text-xs" tickFormatter={(value) => `${value}%`} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--card))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "8px",
                  }}
                  formatter={(value: number) => [`${value}%`, "Attendance"]}
                />
                <Line
                  type="monotone"
                  dataKey="attendance"
                  name="Attendance %"
                  stroke="hsl(var(--primary))"
                  strokeWidth={3}
                  dot={{ fill: "hsl(var(--primary))", strokeWidth: 2, r: 4 }}
                  activeDot={{ r: 6, fill: "hsl(var(--primary))" }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </CardContent>
      </Card>

      {/* Two Column Layout */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Pending Leave Requests */}
        <Card className="hrms-card">
          <CardHeader className="pb-3">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Calendar className="h-5 w-5 text-primary" />
              Pending Leave Requests
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {recentLeaveRequests.map((request) => (
              <div
                key={request.id}
                className="flex items-center justify-between p-3 rounded-lg bg-muted/50"
              >
                <div>
                  <p className="font-medium text-sm">{request.name}</p>
                  <p className="text-xs text-muted-foreground">
                    {request.type} • {request.dates}
                  </p>
                </div>
                <StatusBadge variant={request.status} />
              </div>
            ))}
            <Link to="/admin/leave-approval">
              <button className="w-full text-sm text-primary font-medium py-2 hover:bg-primary/5 rounded-lg transition-colors">
                View All Requests →
              </button>
            </Link>
          </CardContent>
        </Card>

        {/* Today's Attendance */}
        <Card className="hrms-card">
          <CardHeader className="pb-3">
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-primary" />
              Today's Attendance
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {todayAttendance.map((record) => (
              <div
                key={record.id}
                className="flex items-center justify-between p-3 rounded-lg bg-muted/50"
              >
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-sm font-medium text-primary">
                      {record.name.charAt(0)}
                    </span>
                  </div>
                  <div>
                    <p className="font-medium text-sm">{record.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {record.department} • {record.time}
                    </p>
                  </div>
                </div>
                <StatusBadge variant={record.status} />
              </div>
            ))}
            <Link to="/admin/reports/attendance">
              <button className="w-full text-sm text-primary font-medium py-2 hover:bg-primary/5 rounded-lg transition-colors">
                View Full Report →
              </button>
            </Link>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card className="hrms-card">
        <CardHeader className="pb-3">
          <CardTitle className="text-lg font-display flex items-center gap-2">
            <AlertCircle className="h-5 w-5 text-primary" />
            Quick Actions
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <Link to="/admin/add-employee">
              <button className="w-full flex flex-col items-center gap-2 p-4 rounded-xl border border-border hover:border-primary hover:bg-primary/5 transition-all">
                <Users className="h-6 w-6 text-primary" />
                <span className="text-sm font-medium">Add Employee</span>
              </button>
            </Link>
            <Link to="/admin/leave-approval">
              <button className="w-full flex flex-col items-center gap-2 p-4 rounded-xl border border-border hover:border-primary hover:bg-primary/5 transition-all">
                <Calendar className="h-6 w-6 text-primary" />
                <span className="text-sm font-medium">Approve Leaves</span>
              </button>
            </Link>
            <Link to="/admin/masters/payroll">
              <button className="w-full flex flex-col items-center gap-2 p-4 rounded-xl border border-border hover:border-primary hover:bg-primary/5 transition-all">
                <TrendingUp className="h-6 w-6 text-primary" />
                <span className="text-sm font-medium">Run Payroll</span>
              </button>
            </Link>
            <Link to="/admin/reports/attendance">
              <button className="w-full flex flex-col items-center gap-2 p-4 rounded-xl border border-border hover:border-primary hover:bg-primary/5 transition-all">
                <Clock className="h-6 w-6 text-primary" />
                <span className="text-sm font-medium">View Reports</span>
              </button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
