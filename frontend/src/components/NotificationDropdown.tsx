import { useState, useEffect } from "react";
import { Bell, Clock, Calendar, Check, X, FileText } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";

interface Notification {
  id: string;
  type: "leave" | "attendance" | "system";
  title: string;
  message: string;
  time: string;
  read: boolean;
}

const mockNotifications: Notification[] = [
  {
    id: "1",
    type: "leave",
    title: "Leave Request",
    message: "Sarah Johnson requested leave for Jan 15-17",
    time: "5 min ago",
    read: false,
  },
  {
    id: "2",
    type: "attendance",
    title: "Late Check-in",
    message: "3 employees arrived late today",
    time: "1 hour ago",
    read: false,
  },
  {
    id: "3",
    type: "system",
    title: "Payroll Reminder",
    message: "Monthly payroll processing due in 2 days",
    time: "2 hours ago",
    read: true,
  },
  {
    id: "4",
    type: "leave",
    title: "Leave Approved",
    message: "Your leave request has been approved",
    time: "Yesterday",
    read: true,
  },
];

const getIcon = (type: string) => {
  switch (type) {
    case "leave":
      return Calendar;
    case "attendance":
      return Clock;
    default:
      return FileText;
  }
};

const getIconColor = (type: string) => {
  switch (type) {
    case "leave":
      return "text-blue-500 bg-blue-500/10";
    case "attendance":
      return "text-orange-500 bg-orange-500/10";
    default:
      return "text-purple-500 bg-purple-500/10";
  }
};

export function NotificationDropdown() {
  const [notifications, setNotifications] = useState(mockNotifications);
  const [open, setOpen] = useState(false);

  const unreadCount = notifications.filter((n) => !n.read).length;

  const markAsRead = (id: string) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, read: true } : n))
    );
  };

  const markAllAsRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  };

  const removeNotification = (id: string) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-5 w-5" />
          {unreadCount > 0 && (
            <span className="absolute top-1 right-1 w-2 h-2 bg-destructive rounded-full" />
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent
        className="w-80 p-0 bg-card border border-border shadow-lg"
        align="end"
        sideOffset={8}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-2">
            <Bell className="h-4 w-4 text-primary" />
            <span className="font-semibold text-sm">Notifications</span>
            {unreadCount > 0 && (
              <span className="px-1.5 py-0.5 text-xs bg-primary text-primary-foreground rounded-full">
                {unreadCount}
              </span>
            )}
          </div>
          {unreadCount > 0 && (
            <Button
              variant="ghost"
              size="sm"
              className="text-xs h-7"
              onClick={markAllAsRead}
            >
              Mark all read
            </Button>
          )}
        </div>

        {/* Notifications List */}
        <ScrollArea className="max-h-80">
          {notifications.length === 0 ? (
            <div className="p-8 text-center">
              <Bell className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
              <p className="text-sm text-muted-foreground">No notifications</p>
            </div>
          ) : (
            <div className="divide-y divide-border">
              {notifications.map((notification) => {
                const Icon = getIcon(notification.type);
                return (
                  <div
                    key={notification.id}
                    className={cn(
                      "p-3 hover:bg-muted/50 transition-colors cursor-pointer",
                      !notification.read && "bg-primary/5"
                    )}
                    onClick={() => markAsRead(notification.id)}
                  >
                    <div className="flex gap-3">
                      <div
                        className={cn(
                          "w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0",
                          getIconColor(notification.type)
                        )}
                      >
                        <Icon className="h-4 w-4" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between gap-2">
                          <p className="text-sm font-medium truncate">
                            {notification.title}
                          </p>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-5 w-5 -mt-0.5 opacity-0 group-hover:opacity-100 hover:opacity-100"
                            onClick={(e) => {
                              e.stopPropagation();
                              removeNotification(notification.id);
                            }}
                          >
                            <X className="h-3 w-3" />
                          </Button>
                        </div>
                        <p className="text-xs text-muted-foreground line-clamp-2">
                          {notification.message}
                        </p>
                        <p className="text-xs text-muted-foreground mt-1">
                          {notification.time}
                        </p>
                      </div>
                      {!notification.read && (
                        <div className="w-2 h-2 bg-primary rounded-full flex-shrink-0 mt-2" />
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </ScrollArea>

        {/* Footer */}
        {notifications.length > 0 && (
          <div className="p-2 border-t border-border">
            <Button
              variant="ghost"
              className="w-full text-sm text-primary h-8"
              onClick={() => setOpen(false)}
            >
              View all notifications
            </Button>
          </div>
        )}
      </PopoverContent>
    </Popover>
  );
}
