import { useState, useEffect, useCallback } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/auth-context";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import {
  Clock,
  Calendar,
  Play,
  Pause,
  LogIn,
  LogOut,
  Coffee,
  FileText,
  TrendingUp,
  BarChart3,
} from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  LineChart,
  Line,
} from "recharts";

const WORK_DAY_HOURS = 8;
const WORK_DAY_SECONDS = WORK_DAY_HOURS * 3600;

const recentActivity = [
  { type: "attendance", date: "Jan 6, 2026", status: "present", detail: "09:00 AM - 06:05 PM", hours: "9h 5m" },
  { type: "leave", date: "Jan 3, 2026", status: "approved", detail: "Casual Leave", days: "1 day" },
  { type: "attendance", date: "Jan 2, 2026", status: "present", detail: "09:15 AM - 06:00 PM", hours: "8h 45m", isLate: true },
];

// Weekly attendance data
const weeklyAttendanceData = [
  { day: "Mon", hours: 8.5 },
  { day: "Tue", hours: 9.0 },
  { day: "Wed", hours: 8.0 },
  { day: "Thu", hours: 8.75 },
  { day: "Fri", hours: 8.25 },
  { day: "Sat", hours: 0 },
  { day: "Sun", hours: 0 },
];

// Monthly attendance data
const monthlyAttendanceData = [
  { month: "Jan", attendance: 95 },
  { month: "Feb", attendance: 92 },
  { month: "Mar", attendance: 98 },
  { month: "Apr", attendance: 94 },
  { month: "May", attendance: 96 },
  { month: "Jun", attendance: 93 },
  { month: "Jul", attendance: 97 },
  { month: "Aug", attendance: 95 },
  { month: "Sep", attendance: 94 },
  { month: "Oct", attendance: 96 },
  { month: "Nov", attendance: 92 },
  { month: "Dec", attendance: 95 },
];

export default function EmployeeDashboard() {
  const { user } = useAuth();
  const navigate = useNavigate();
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
      toast.info("Break ended. Welcome back!");
    } else {
      toast.info("Break started. Enjoy!");
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

  const workingTime = elapsedTime - totalBreakTime - (onBreak ? breakTime : 0);
  const currentBreakDisplay = onBreak ? breakTime : 0;
  const allBreakTime = totalBreakTime + currentBreakDisplay;
  const progressPercent = Math.min((workingTime / WORK_DAY_SECONDS) * 100, 100);
  const circumference = 2 * Math.PI * 120;
  const strokeDashoffset = circumference - (progressPercent / 100) * circumference;

  const currentDate = new Date();
  const dateString = currentDate.toLocaleDateString('en-US', { 
    weekday: 'long', 
    month: 'long', 
    day: 'numeric', 
    year: 'numeric' 
  });

  const handleActivityClick = (type: string) => {
    if (type === "attendance") {
      navigate("/employee/attendance");
    } else {
      navigate("/employee/leave");
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Minimal Greeting */}
      <div className="text-center pt-4">
        <p className="text-muted-foreground text-sm">
          {dateString}
        </p>
        <h1 className="text-2xl font-display font-bold mt-1">
          Hello, {user?.name.split(' ')[0]}
        </h1>
      </div>

      {/* Circular Clock Card with Progress Ring */}
      <div className="flex justify-center py-6">
        <div className="relative">
          {/* SVG Progress Ring */}
          <svg className="w-64 h-64 -rotate-90" viewBox="0 0 256 256">
            {/* Background Circle */}
            <circle
              cx="128"
              cy="128"
              r="120"
              fill="none"
              strokeWidth="8"
              className="stroke-border"
            />
            {/* Progress Circle with pulse animation */}
            {clockedIn && (
              <circle
                cx="128"
                cy="128"
                r="120"
                fill="none"
                strokeWidth="8"
                strokeLinecap="round"
                className={cn(
                  "transition-all duration-1000",
                  onBreak ? "stroke-warning" : "stroke-primary",
                  !onBreak && "animate-[pulse_2s_ease-in-out_infinite]"
                )}
                style={{
                  strokeDasharray: circumference,
                  strokeDashoffset: strokeDashoffset,
                }}
              />
            )}
            {/* Glow effect when working */}
            {clockedIn && !onBreak && (
              <circle
                cx="128"
                cy="128"
                r="120"
                fill="none"
                strokeWidth="12"
                strokeLinecap="round"
                className="stroke-primary/20 blur-sm animate-[pulse_2s_ease-in-out_infinite]"
                style={{
                  strokeDasharray: circumference,
                  strokeDashoffset: strokeDashoffset,
                }}
              />
            )}
          </svg>

          {/* Inner Content */}
          <div className="absolute inset-0 flex items-center justify-center">
            <div className={cn(
              "w-52 h-52 rounded-full flex items-center justify-center transition-all duration-300",
              clockedIn 
                ? onBreak 
                  ? "bg-warning/5" 
                  : "bg-primary/5"
                : "bg-card"
            )}>
              <div className="text-center space-y-2">
                {clockedIn ? (
                  <>
                    {/* Status */}
                    <div className={cn(
                      "inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-medium",
                      onBreak 
                        ? "bg-warning/20 text-warning-foreground" 
                        : "bg-primary/20 text-primary"
                    )}>
                      {onBreak ? <Coffee className="h-3 w-3" /> : <Clock className="h-3 w-3" />}
                      {onBreak ? "On Break" : "Working"}
                    </div>
                    
                    {/* Timer */}
                    <p className="text-3xl font-mono font-bold text-foreground">
                      {formatTime(elapsedTime)}
                    </p>
                    
                    {/* Progress */}
                    <p className="text-xs text-muted-foreground">
                      {Math.round(progressPercent)}% of {WORK_DAY_HOURS}h completed
                    </p>

                    {/* Clock In Time */}
                    <p className="text-xs text-muted-foreground">
                      Since {clockInTime?.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
                    </p>
                  </>
                ) : (
                  <>
                    <Clock className="h-10 w-10 text-muted-foreground mx-auto" />
                    <p className="text-base font-medium text-muted-foreground">
                      Not Clocked In
                    </p>
                    <p className="text-xs text-muted-foreground">
                      Tap to start
                    </p>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-center gap-3">
        {clockedIn ? (
          <>
            {/* Break Button */}
            <Button
              onClick={toggleBreak}
              variant="outline"
              className={cn(
                "gap-2",
                onBreak 
                  ? "border-primary text-primary hover:bg-primary hover:text-primary-foreground" 
                  : "border-warning text-warning hover:bg-warning hover:text-warning-foreground"
              )}
            >
              {onBreak ? <Play className="h-4 w-4" /> : <Pause className="h-4 w-4" />}
              {onBreak ? "Resume" : "Break"}
              {allBreakTime > 0 && (
                <span className="text-xs opacity-80">({formatTimeShort(allBreakTime)})</span>
              )}
            </Button>

            {/* Clock Out Button */}
            <Button
              onClick={handleClockOut}
              variant="outline"
              className="gap-2 border-destructive text-destructive hover:bg-destructive hover:text-destructive-foreground"
            >
              <LogOut className="h-4 w-4" />
              Clock Out
            </Button>
          </>
        ) : (
          <Button
            onClick={handleClockIn}
            className="gap-2 gradient-primary text-primary-foreground px-8"
          >
            <LogIn className="h-4 w-4" />
            Clock In
          </Button>
        )}
      </div>

      {/* Break Time Display */}
      {clockedIn && allBreakTime > 0 && (
        <div className="flex justify-center">
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-muted/50 text-sm">
            <Coffee className="h-4 w-4 text-warning" />
            <span className="text-muted-foreground">Total break:</span>
            <span className="font-medium">{formatTimeShort(allBreakTime)}</span>
          </div>
        </div>
      )}

      {/* Attendance Statistics - Only show when clocked in */}
      {clockedIn && (
        <div className="grid grid-cols-3 gap-3 pt-2">
          <Card className="hrms-card">
            <CardContent className="p-4 text-center">
              <Clock className="h-5 w-5 text-primary mx-auto mb-2" />
              <p className="text-2xl font-bold">{formatTimeShort(workingTime)}</p>
              <p className="text-xs text-muted-foreground">Working Hours</p>
            </CardContent>
          </Card>
          
          <Card className="hrms-card">
            <CardContent className="p-4 text-center">
              <Coffee className="h-5 w-5 text-warning mx-auto mb-2" />
              <p className="text-2xl font-bold">{formatTimeShort(allBreakTime)}</p>
              <p className="text-xs text-muted-foreground">Break Time</p>
            </CardContent>
          </Card>
          
          <Card className="hrms-card">
            <CardContent className="p-4 text-center">
              <TrendingUp className="h-5 w-5 text-success mx-auto mb-2" />
              <p className="text-2xl font-bold">8.5h</p>
              <p className="text-xs text-muted-foreground">Avg Hours</p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Attendance Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Weekly Attendance */}
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-base font-display flex items-center gap-2">
              <BarChart3 className="h-4 w-4 text-primary" />
              Weekly Hours
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-48">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={weeklyAttendanceData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                  <XAxis dataKey="day" className="text-xs" />
                  <YAxis domain={[0, 10]} className="text-xs" tickFormatter={(value) => `${value}h`} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "hsl(var(--card))",
                      border: "1px solid hsl(var(--border))",
                      borderRadius: "8px",
                    }}
                    formatter={(value: number) => [`${value}h`, "Hours"]}
                  />
                  <Bar
                    dataKey="hours"
                    fill="hsl(var(--primary))"
                    radius={[4, 4, 0, 0]}
                  />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        {/* Monthly Attendance */}
        <Card className="hrms-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-base font-display flex items-center gap-2">
              <TrendingUp className="h-4 w-4 text-primary" />
              Monthly Attendance
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="h-48">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={monthlyAttendanceData}>
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
                    stroke="hsl(var(--primary))"
                    strokeWidth={2}
                    dot={{ fill: "hsl(var(--primary))", strokeWidth: 2, r: 3 }}
                    activeDot={{ r: 5, fill: "hsl(var(--primary))" }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent Activity */}
      <Card className="hrms-card">
        <CardHeader className="pb-3">
          <CardTitle className="text-base font-display flex items-center gap-2">
            <FileText className="h-4 w-4 text-primary" />
            Recent Activity
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          {recentActivity.map((activity, index) => (
            <div
              key={index}
              onClick={() => handleActivityClick(activity.type)}
              className="flex items-center justify-between p-3 rounded-lg bg-muted/50 cursor-pointer hover:bg-muted transition-colors"
            >
              <div className="flex items-center gap-3">
                <div className={cn(
                  "w-8 h-8 rounded-full flex items-center justify-center",
                  activity.type === "attendance" 
                    ? "bg-primary/10" 
                    : "bg-success/10"
                )}>
                  {activity.type === "attendance" ? (
                    <Clock className="h-4 w-4 text-primary" />
                  ) : (
                    <Calendar className="h-4 w-4 text-success" />
                  )}
                </div>
                <div>
                  <p className="text-sm font-medium">{activity.date}</p>
                  <p className="text-xs text-muted-foreground">{activity.detail}</p>
                </div>
              </div>
              <div className="flex items-center gap-2">
                {activity.isLate && <StatusBadge variant="late" />}
                {activity.type === "attendance" ? (
                  <StatusBadge variant={activity.status as "present" | "absent"} />
                ) : (
                  <StatusBadge variant={activity.status as "approved" | "pending" | "rejected"} />
                )}
                {activity.hours && (
                  <span className="text-xs font-medium text-primary ml-1">
                    {activity.hours}
                  </span>
                )}
                {activity.days && (
                  <span className="text-xs font-medium text-muted-foreground ml-1">
                    {activity.days}
                  </span>
                )}
              </div>
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
}
