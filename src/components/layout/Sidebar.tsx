import { cn } from "@/lib/utils";
import { useAuth, UserRole } from "@/lib/auth-context";
import { Link, useLocation } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  UserPlus,
  ClipboardCheck,
  Calendar,
  Settings,
  Building2,
  Clock,
  CreditCard,
  BarChart3,
  LogOut,
  ChevronDown,
  X,
  Bell,
  User,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { useState } from "react";

interface NavItem {
  title: string;
  href?: string;
  icon: React.ElementType;
  children?: { title: string; href: string }[];
  roles: UserRole[];
}

const navigation: NavItem[] = [
  {
    title: "Dashboard",
    href: "/admin",
    icon: LayoutDashboard,
    roles: ["admin"],
  },
  {
    title: "Dashboard",
    href: "/employee",
    icon: LayoutDashboard,
    roles: ["employee"],
  },
  {
    title: "User Management",
    href: "/admin/users",
    icon: Users,
    roles: ["admin"],
  },
  {
    title: "Add Employee",
    href: "/admin/add-employee",
    icon: UserPlus,
    roles: ["admin"],
  },
  {
    title: "Leave Approval",
    href: "/admin/leave-approval",
    icon: ClipboardCheck,
    roles: ["admin"],
  },
  {
    title: "Masters",
    icon: Settings,
    roles: ["admin"],
    children: [
      { title: "Department", href: "/admin/masters/department" },
      { title: "Designation", href: "/admin/masters/designation" },
      { title: "Leave Type", href: "/admin/masters/leave-type" },
      { title: "Holiday", href: "/admin/masters/holiday" },
      { title: "Shift", href: "/admin/masters/shift" },
      { title: "Category", href: "/admin/masters/category" },
      { title: "Company", href: "/admin/masters/company" },
      { title: "Employee", href: "/admin/masters/employee" },
      { title: "Machine", href: "/admin/masters/machine" },
      { title: "Payroll", href: "/admin/masters/payroll" },
    ],
  },
  {
    title: "Reports",
    icon: BarChart3,
    roles: ["admin"],
    children: [
      { title: "Attendance", href: "/admin/reports/attendance" },
      { title: "Leave", href: "/admin/reports/leave" },
      { title: "Payroll", href: "/admin/reports/payroll" },
      { title: "Employee", href: "/admin/reports/employee" },
    ],
  },
  {
    title: "My Attendance",
    href: "/employee/attendance",
    icon: Clock,
    roles: ["employee"],
  },
  {
    title: "Leave Management",
    href: "/employee/leave",
    icon: Calendar,
    roles: ["employee"],
  },
  {
    title: "Payroll",
    href: "/employee/payroll",
    icon: CreditCard,
    roles: ["employee"],
  },
  {
    title: "Employee Directory",
    href: "/employee/directory",
    icon: Users,
    roles: ["employee"],
  },
  {
    title: "Notifications",
    href: "/admin/notifications",
    icon: Bell,
    roles: ["admin"],
  },
  {
    title: "Notifications",
    href: "/employee/notifications",
    icon: Bell,
    roles: ["employee"],
  },
  {
    title: "My Profile",
    href: "/admin/profile",
    icon: User,
    roles: ["admin"],
  },
  {
    title: "My Profile",
    href: "/employee/profile",
    icon: User,
    roles: ["employee"],
  },
];

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [expandedItems, setExpandedItems] = useState<string[]>(["Masters", "Reports"]);

  const toggleExpanded = (title: string) => {
    setExpandedItems((prev) =>
      prev.includes(title)
        ? prev.filter((item) => item !== title)
        : [...prev, title]
    );
  };

  const filteredNavigation = navigation.filter(
    (item) => user && item.roles.includes(user.role)
  );

  const isActive = (href?: string) => {
    if (!href) return false;
    return location.pathname === href;
  };

  const isChildActive = (children?: { href: string }[]) => {
    if (!children) return false;
    return children.some((child) => location.pathname === child.href);
  };

  return (
    <>
      {/* Overlay */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-foreground/20 backdrop-blur-sm z-40"
          onClick={onClose}
        />
      )}

      {/* Sidebar - Always slides in from left */}
      <aside
        className={cn(
          "fixed top-0 left-0 z-50 h-full w-72 bg-sidebar border-r border-sidebar-border",
          "transform transition-transform duration-300 ease-in-out",
          isOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="flex items-center justify-between p-5 border-b border-sidebar-border">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl gradient-primary flex items-center justify-center">
                <Building2 className="w-5 h-5 text-primary-foreground" />
              </div>
              <div>
                <h2 className="font-display font-bold text-lg text-sidebar-foreground">
                  SFON HRMS
                </h2>
                <p className="text-xs text-muted-foreground capitalize">
                  {user?.role} Portal
                </p>
              </div>
            </div>
            <Button
              variant="ghost"
              size="icon"
              onClick={onClose}
            >
              <X className="h-5 w-5" />
            </Button>
          </div>

          {/* Navigation */}
          <nav className="flex-1 overflow-y-auto p-4 space-y-1">
            {filteredNavigation.map((item) => (
              <div key={item.title}>
                {item.children ? (
                  <div>
                    <button
                      onClick={() => toggleExpanded(item.title)}
                      className={cn(
                        "w-full flex items-center justify-between gap-3 px-3 py-2.5 rounded-lg",
                        "text-sidebar-foreground hover:bg-sidebar-accent transition-colors",
                        isChildActive(item.children) && "bg-sidebar-accent"
                      )}
                    >
                      <div className="flex items-center gap-3">
                        <item.icon className="h-5 w-5" />
                        <span className="font-medium text-sm">{item.title}</span>
                      </div>
                      <ChevronDown
                        className={cn(
                          "h-4 w-4 transition-transform",
                          expandedItems.includes(item.title) && "rotate-180"
                        )}
                      />
                    </button>
                    {expandedItems.includes(item.title) && (
                      <div className="mt-1 ml-4 pl-4 border-l border-sidebar-border space-y-1">
                        {item.children.map((child) => (
                          <Link
                            key={child.href}
                            to={child.href}
                            onClick={onClose}
                            className={cn(
                              "block px-3 py-2 rounded-lg text-sm transition-colors",
                              isActive(child.href)
                                ? "bg-primary text-primary-foreground font-medium"
                                : "text-sidebar-foreground hover:bg-sidebar-accent"
                            )}
                          >
                            {child.title}
                          </Link>
                        ))}
                      </div>
                    )}
                  </div>
                ) : (
                  <Link
                    to={item.href!}
                    onClick={onClose}
                    className={cn(
                      "flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors",
                      isActive(item.href)
                        ? "bg-primary text-primary-foreground"
                        : "text-sidebar-foreground hover:bg-sidebar-accent"
                    )}
                  >
                    <item.icon className="h-5 w-5" />
                    <span className="font-medium text-sm">{item.title}</span>
                  </Link>
                )}
              </div>
            ))}
          </nav>

          {/* User Section */}
          <div className="p-4 border-t border-sidebar-border">
            <div className="flex items-center gap-3 mb-3">
              <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center">
                <span className="text-sm font-semibold text-primary">
                  {user?.name.charAt(0)}
                </span>
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-sidebar-foreground truncate">
                  {user?.name}
                </p>
                <p className="text-xs text-muted-foreground truncate">
                  {user?.email}
                </p>
              </div>
            </div>
            <Button
              variant="outline"
              className="w-full justify-start gap-2"
              onClick={logout}
            >
              <LogOut className="h-4 w-4" />
              Sign Out
            </Button>
          </div>
        </div>
      </aside>
    </>
  );
}
