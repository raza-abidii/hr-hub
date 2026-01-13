import { cn } from "@/lib/utils";
import { Check, X, Clock, Calendar, AlertTriangle } from "lucide-react";

type StatusVariant = 
  | 'present' 
  | 'absent' 
  | 'pending' 
  | 'approved' 
  | 'rejected' 
  | 'weekend' 
  | 'holiday' 
  | 'leave'
  | 'late'
  | 'early'
  | 'active'
  | 'inactive';

interface StatusBadgeProps {
  variant: StatusVariant;
  children?: React.ReactNode;
  showIcon?: boolean;
  className?: string;
}

const statusConfig: Record<StatusVariant, { 
  bg: string; 
  text: string; 
  icon?: React.ReactNode;
  label: string;
}> = {
  present: {
    bg: 'bg-success/10',
    text: 'text-success',
    icon: <Check className="w-3 h-3" />,
    label: 'Present',
  },
  absent: {
    bg: 'bg-destructive/10',
    text: 'text-destructive',
    icon: <X className="w-3 h-3" />,
    label: 'Absent',
  },
  pending: {
    bg: 'bg-warning/15',
    text: 'text-warning',
    icon: <Clock className="w-3 h-3" />,
    label: 'Pending',
  },
  approved: {
    bg: 'bg-success/10',
    text: 'text-success',
    icon: <Check className="w-3 h-3" />,
    label: 'Approved',
  },
  rejected: {
    bg: 'bg-destructive/10',
    text: 'text-destructive',
    icon: <X className="w-3 h-3" />,
    label: 'Rejected',
  },
  weekend: {
    bg: 'bg-muted',
    text: 'text-muted-foreground',
    label: 'Weekend',
  },
  holiday: {
    bg: 'bg-info/10',
    text: 'text-info',
    icon: <Calendar className="w-3 h-3" />,
    label: 'Holiday',
  },
  leave: {
    bg: 'bg-purple-100',
    text: 'text-purple-700',
    icon: <Calendar className="w-3 h-3" />,
    label: 'On Leave',
  },
  late: {
    bg: 'bg-warning/15',
    text: 'text-warning',
    icon: <AlertTriangle className="w-3 h-3" />,
    label: 'Late In',
  },
  early: {
    bg: 'bg-warning/15',
    text: 'text-warning',
    icon: <AlertTriangle className="w-3 h-3" />,
    label: 'Early Out',
  },
  active: {
    bg: 'bg-success/10',
    text: 'text-success',
    label: 'Active',
  },
  inactive: {
    bg: 'bg-muted',
    text: 'text-muted-foreground',
    label: 'Inactive',
  },
};

export function StatusBadge({ variant, children, showIcon = true, className }: StatusBadgeProps) {
  const config = statusConfig[variant];

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium",
        config.bg,
        config.text,
        className
      )}
    >
      {showIcon && config.icon}
      {children || config.label}
    </span>
  );
}
