import { cn } from "@/lib/utils";
import { LucideIcon } from "lucide-react";

interface StatCardProps {
  title: string;
  value: string | number;
  icon?: LucideIcon;
  trend?: {
    value: number;
    isPositive: boolean;
  };
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info';
  className?: string;
}

const variantStyles = {
  default: {
    icon: 'bg-primary/10 text-primary',
    value: 'text-foreground',
  },
  success: {
    icon: 'bg-success/10 text-success',
    value: 'text-success',
  },
  warning: {
    icon: 'bg-warning/10 text-warning',
    value: 'text-warning',
  },
  danger: {
    icon: 'bg-destructive/10 text-destructive',
    value: 'text-destructive',
  },
  info: {
    icon: 'bg-info/10 text-info',
    value: 'text-info',
  },
};

export function StatCard({ 
  title, 
  value, 
  icon: Icon, 
  trend, 
  variant = 'default',
  className 
}: StatCardProps) {
  const styles = variantStyles[variant];

  return (
    <div className={cn(
      "hrms-card p-5 flex items-start justify-between gap-4",
      className
    )}>
      <div className="space-y-1.5">
        <p className="text-sm font-medium text-muted-foreground">{title}</p>
        <p className={cn("text-2xl font-display font-bold", styles.value)}>
          {value}
        </p>
        {trend && (
          <p className={cn(
            "text-xs font-medium",
            trend.isPositive ? "text-success" : "text-destructive"
          )}>
            {trend.isPositive ? "+" : ""}{trend.value}% from last month
          </p>
        )}
      </div>
      {Icon && (
        <div className={cn("p-3 rounded-xl", styles.icon)}>
          <Icon className="h-5 w-5" />
        </div>
      )}
    </div>
  );
}
