import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Bell,
  Calendar,
  Clock,
  CheckCircle,
  AlertCircle,
  UserCheck,
  Check,
  X,
  Trash2,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useToast } from "@/hooks/use-toast";

interface Notification {
  id: string;
  type: 'leave' | 'attendance' | 'system' | 'approval';
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  actionable?: boolean;
  priority?: 'high' | 'medium' | 'low';
}

const mockNotifications: Notification[] = [
  {
    id: "1",
    type: "leave",
    title: "Leave Request Submitted",
    message: "Your leave request for Jan 15-17 has been submitted for approval.",
    timestamp: "10 minutes ago",
    read: false,
    priority: "medium",
  },
  {
    id: "2",
    type: "approval",
    title: "Leave Approved",
    message: "Your leave request for Dec 20-22 has been approved by your manager.",
    timestamp: "2 hours ago",
    read: false,
    priority: "high",
  },
  {
    id: "3",
    type: "attendance",
    title: "Late Check-in Alert",
    message: "You checked in 15 minutes late today at 9:15 AM.",
    timestamp: "5 hours ago",
    read: true,
    priority: "low",
  },
  {
    id: "4",
    type: "system",
    title: "Holiday Announcement",
    message: "Office will be closed on Jan 26 for Republic Day.",
    timestamp: "1 day ago",
    read: true,
  },
  {
    id: "5",
    type: "attendance",
    title: "Attendance Reminder",
    message: "Don't forget to clock out before leaving today.",
    timestamp: "1 day ago",
    read: true,
  },
  {
    id: "6",
    type: "system",
    title: "Policy Update",
    message: "Updated leave policy effective from Feb 1. Please review the changes.",
    timestamp: "2 days ago",
    read: true,
    actionable: true,
  },
  {
    id: "7",
    type: "leave",
    title: "Leave Balance Updated",
    message: "Your leave balance has been updated for the new fiscal year.",
    timestamp: "3 days ago",
    read: true,
  },
];

const typeConfig = {
  leave: {
    icon: Calendar,
    color: "text-blue-500",
    bg: "bg-blue-500/10",
  },
  attendance: {
    icon: Clock,
    color: "text-orange-500",
    bg: "bg-orange-500/10",
  },
  system: {
    icon: AlertCircle,
    color: "text-purple-500",
    bg: "bg-purple-500/10",
  },
  approval: {
    icon: UserCheck,
    color: "text-green-500",
    bg: "bg-green-500/10",
  },
};

export default function Notifications() {
  const [notifications, setNotifications] = useState(mockNotifications);
  const [activeTab, setActiveTab] = useState("all");
  const { toast } = useToast();

  const unreadCount = notifications.filter((n) => !n.read).length;

  const filteredNotifications =
    activeTab === "all"
      ? notifications
      : notifications.filter((n) => n.type === activeTab);

  const markAsRead = (id: string) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, read: true } : n))
    );
  };

  const markAllAsRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
    toast({
      title: "Done",
      description: "All notifications marked as read.",
    });
  };

  const deleteNotification = (id: string) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
    toast({
      title: "Deleted",
      description: "Notification removed.",
    });
  };

  const getTabCount = (type: string) => {
    if (type === "all") return notifications.filter((n) => !n.read).length;
    return notifications.filter((n) => n.type === type && !n.read).length;
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Notifications"
        subtitle={`You have ${unreadCount} unread notification${unreadCount !== 1 ? 's' : ''}`}
        action={
          unreadCount > 0 && (
            <Button variant="outline" onClick={markAllAsRead}>
              <Check className="h-4 w-4 mr-2" />
              Mark All as Read
            </Button>
          )
        }
      />

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-5 lg:w-auto lg:inline-grid">
          <TabsTrigger value="all" className="relative">
            All
            {getTabCount("all") > 0 && (
              <span className="absolute -top-1 -right-1 w-4 h-4 bg-destructive text-destructive-foreground text-xs rounded-full flex items-center justify-center">
                {getTabCount("all")}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="leave" className="relative">
            Leave
            {getTabCount("leave") > 0 && (
              <span className="absolute -top-1 -right-1 w-4 h-4 bg-destructive text-destructive-foreground text-xs rounded-full flex items-center justify-center">
                {getTabCount("leave")}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="attendance">Attendance</TabsTrigger>
          <TabsTrigger value="approval">Approvals</TabsTrigger>
          <TabsTrigger value="system">System</TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="mt-6">
          {filteredNotifications.length > 0 ? (
            <Card className="hrms-card">
              <CardContent className="p-0 divide-y divide-border">
                {filteredNotifications.map((notification) => {
                  const config = typeConfig[notification.type];
                  const Icon = config.icon;

                  return (
                    <div
                      key={notification.id}
                      className={cn(
                        "p-4 flex gap-4 hover:bg-muted/30 transition-colors cursor-pointer",
                        !notification.read && "bg-accent/30 border-l-4 border-l-primary"
                      )}
                      onClick={() => markAsRead(notification.id)}
                    >
                      <div className={cn("p-2.5 rounded-xl h-fit", config.bg)}>
                        <Icon className={cn("h-5 w-5", config.color)} />
                      </div>

                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between gap-2">
                          <div>
                            <h4
                              className={cn(
                                "text-sm",
                                !notification.read && "font-semibold"
                              )}
                            >
                              {notification.title}
                            </h4>
                            <p className="text-sm text-muted-foreground mt-0.5">
                              {notification.message}
                            </p>
                          </div>
                          <div className="flex items-center gap-1">
                            {!notification.read && (
                              <span className="w-2 h-2 bg-primary rounded-full" />
                            )}
                            <Button
                              variant="ghost"
                              size="icon"
                              className="h-8 w-8 text-muted-foreground hover:text-destructive"
                              onClick={(e) => {
                                e.stopPropagation();
                                deleteNotification(notification.id);
                              }}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
                        </div>

                        <p className="text-xs text-muted-foreground mt-2">
                          {notification.timestamp}
                        </p>

                        {notification.actionable && (
                          <div className="mt-3 flex gap-2">
                            <Button size="sm" className="gradient-primary">
                              View Details
                            </Button>
                          </div>
                        )}
                      </div>
                    </div>
                  );
                })}
              </CardContent>
            </Card>
          ) : (
            <div className="text-center py-12">
              <Bell className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-lg font-medium">No notifications</p>
              <p className="text-muted-foreground">You're all caught up!</p>
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
