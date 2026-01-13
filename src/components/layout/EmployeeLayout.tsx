import { Outlet, Navigate, Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@/lib/auth-context";
import { Button } from "@/components/ui/button";
import { Home, Clock, Calendar, CreditCard, Users, LogOut } from "lucide-react";
import { cn } from "@/lib/utils";
import { NotificationDropdown } from "@/components/NotificationDropdown";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const bottomNavItems = [
  { title: "Home", href: "/employee", icon: Home },
  { title: "Attendance", href: "/employee/attendance", icon: Clock },
  { title: "Leave", href: "/employee/leave", icon: Calendar },
  { title: "Payroll", href: "/employee/payroll", icon: CreditCard },
  { title: "Directory", href: "/employee/directory", icon: Users },
];

export function EmployeeLayout() {
  const { isAuthenticated, user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const isActive = (href: string) => {
    if (href === "/employee") return location.pathname === "/employee";
    return location.pathname.startsWith(href);
  };

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-background flex flex-col">
      {/* Top Header */}
      <header className="sticky top-0 z-30 h-14 bg-card border-b border-border flex items-center justify-between px-4">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center">
            <span className="text-xs font-bold text-primary-foreground">SF</span>
          </div>
          <span className="font-display font-bold text-lg">SFON HRMS</span>
        </div>

        <div className="flex items-center gap-2">
          <NotificationDropdown />
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button className="w-8 h-8 rounded-full gradient-primary flex items-center justify-center focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2">
                <span className="text-xs font-semibold text-primary-foreground">
                  {user?.name.charAt(0)}
                </span>
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-48 bg-popover">
              <DropdownMenuItem asChild>
                <Link to="/employee/profile" className="cursor-pointer">
                  Profile
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout} className="text-destructive cursor-pointer">
                <LogOut className="h-4 w-4 mr-2" />
                Log Out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 p-4 overflow-auto pb-20">
        <Outlet />
      </main>

      {/* Bottom Navigation */}
      <nav className="fixed bottom-0 left-0 right-0 h-16 bg-card border-t border-border flex items-center justify-around px-2 z-30">
        {bottomNavItems.map((item) => (
          <Link
            key={item.href}
            to={item.href}
            className={cn(
              "flex flex-col items-center gap-1 py-2 px-3 rounded-lg transition-colors",
              isActive(item.href)
                ? "text-primary"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            <item.icon className="h-5 w-5" />
            <span className="text-xs font-medium">{item.title}</span>
          </Link>
        ))}
      </nav>
    </div>
  );
}
