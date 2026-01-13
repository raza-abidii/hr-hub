import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { StatCard } from "@/components/ui/stat-card";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Download,
  DollarSign,
  TrendingUp,
  TrendingDown,
  CreditCard,
  Building2,
  FileText,
  Mail,
} from "lucide-react";
import { StatusBadge } from "@/components/ui/status-badge";
import { useToast } from "@/hooks/use-toast";

interface PayslipMonth {
  month: string;
  year: number;
  gross: number;
  deductions: number;
  net: number;
  status: 'paid' | 'pending' | 'processing';
  paidDate?: string;
}

const payslipHistory: PayslipMonth[] = [
  { month: "January", year: 2026, gross: 85000, deductions: 17000, net: 68000, status: "pending" },
  { month: "December", year: 2025, gross: 85000, deductions: 17000, net: 68000, status: "paid", paidDate: "Dec 30, 2025" },
  { month: "November", year: 2025, gross: 85000, deductions: 17000, net: 68000, status: "paid", paidDate: "Nov 30, 2025" },
  { month: "October", year: 2025, gross: 82000, deductions: 16400, net: 65600, status: "paid", paidDate: "Oct 30, 2025" },
  { month: "September", year: 2025, gross: 82000, deductions: 16400, net: 65600, status: "paid", paidDate: "Sep 30, 2025" },
];

const currentPayslip = {
  month: "January 2026",
  status: "pending" as const,
  earnings: [
    { component: "Basic Salary", amount: 42500 },
    { component: "House Rent Allowance", amount: 17000 },
    { component: "Special Allowance", amount: 15000 },
    { component: "Conveyance Allowance", amount: 3000 },
    { component: "Performance Bonus", amount: 7500 },
  ],
  deductions: [
    { component: "Provident Fund (EPF)", amount: 5100 },
    { component: "Professional Tax", amount: 200 },
    { component: "Income Tax (TDS)", amount: 10200 },
    { component: "Health Insurance", amount: 1500 },
  ],
};

const bankDetails = {
  bankName: "State Bank of India",
  accountNumber: "****7890",
  ifscCode: "SBIN0001234",
  accountType: "Savings",
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(amount);
};

export default function Payroll() {
  const [selectedMonth, setSelectedMonth] = useState("January 2026");
  const { toast } = useToast();

  const grossSalary = currentPayslip.earnings.reduce((sum, e) => sum + e.amount, 0);
  const totalDeductions = currentPayslip.deductions.reduce((sum, d) => sum + d.amount, 0);
  const netSalary = grossSalary - totalDeductions;

  // YTD calculations
  const ytdEarnings = 850000;
  const ytdDeductions = 170000;
  const ytdNet = 680000;
  const ytdTax = 102000;

  const handleDownload = () => {
    toast({
      title: "Download Started",
      description: "Your payslip is being downloaded...",
    });
  };

  const handleEmailPayslip = () => {
    toast({
      title: "Email Sent",
      description: "Payslip has been sent to your registered email.",
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Payroll"
        subtitle="View your salary and payslips"
      />

      {/* YTD Summary Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="YTD Earnings"
          value={formatCurrency(ytdEarnings)}
          icon={TrendingUp}
          variant="success"
        />
        <StatCard
          title="YTD Deductions"
          value={formatCurrency(ytdDeductions)}
          icon={TrendingDown}
          variant="danger"
        />
        <StatCard
          title="YTD Net Paid"
          value={formatCurrency(ytdNet)}
          icon={CreditCard}
          variant="default"
        />
        <StatCard
          title="Tax Deducted"
          value={formatCurrency(ytdTax)}
          icon={DollarSign}
          variant="warning"
        />
      </div>

      {/* Current Month Payslip */}
      <Card className="hrms-card">
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <div>
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <FileText className="h-5 w-5 text-primary" />
              Current Month Payslip
            </CardTitle>
            <p className="text-sm text-muted-foreground mt-1">
              {currentPayslip.month}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <StatusBadge variant="pending" />
            <Button variant="outline" size="sm" onClick={handleEmailPayslip}>
              <Mail className="h-4 w-4 mr-2" />
              Email
            </Button>
            <Button size="sm" className="gradient-primary" onClick={handleDownload}>
              <Download className="h-4 w-4 mr-2" />
              Download
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid md:grid-cols-2 gap-6">
            {/* Earnings */}
            <div className="space-y-4">
              <h4 className="font-semibold text-success flex items-center gap-2">
                <TrendingUp className="h-4 w-4" />
                Earnings
              </h4>
              <div className="space-y-2">
                {currentPayslip.earnings.map((item) => (
                  <div
                    key={item.component}
                    className="flex justify-between py-2 border-b border-border last:border-0"
                  >
                    <span className="text-sm text-muted-foreground">
                      {item.component}
                    </span>
                    <span className="text-sm font-medium">
                      {formatCurrency(item.amount)}
                    </span>
                  </div>
                ))}
                <div className="flex justify-between py-2 bg-success/10 rounded px-2 mt-2">
                  <span className="font-semibold text-success">Gross Salary</span>
                  <span className="font-bold text-success">
                    {formatCurrency(grossSalary)}
                  </span>
                </div>
              </div>
            </div>

            {/* Deductions */}
            <div className="space-y-4">
              <h4 className="font-semibold text-destructive flex items-center gap-2">
                <TrendingDown className="h-4 w-4" />
                Deductions
              </h4>
              <div className="space-y-2">
                {currentPayslip.deductions.map((item) => (
                  <div
                    key={item.component}
                    className="flex justify-between py-2 border-b border-border last:border-0"
                  >
                    <span className="text-sm text-muted-foreground">
                      {item.component}
                    </span>
                    <span className="text-sm font-medium">
                      {formatCurrency(item.amount)}
                    </span>
                  </div>
                ))}
                <div className="flex justify-between py-2 bg-destructive/10 rounded px-2 mt-2">
                  <span className="font-semibold text-destructive">Total Deductions</span>
                  <span className="font-bold text-destructive">
                    {formatCurrency(totalDeductions)}
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* Net Salary */}
          <div className="mt-6 p-4 rounded-xl gradient-primary text-primary-foreground">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-2">
              <span className="text-lg font-medium opacity-90">Net Payable</span>
              <span className="text-3xl font-display font-bold">
                {formatCurrency(netSalary)}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Payslip History */}
        <Card className="hrms-card lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <FileText className="h-5 w-5 text-primary" />
              Payslip History
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-border bg-muted/50">
                    <th className="text-left p-4 font-medium text-sm text-muted-foreground">
                      Month
                    </th>
                    <th className="text-right p-4 font-medium text-sm text-muted-foreground">
                      Gross
                    </th>
                    <th className="text-right p-4 font-medium text-sm text-muted-foreground">
                      Deductions
                    </th>
                    <th className="text-right p-4 font-medium text-sm text-muted-foreground">
                      Net
                    </th>
                    <th className="text-center p-4 font-medium text-sm text-muted-foreground">
                      Status
                    </th>
                    <th className="text-right p-4 font-medium text-sm text-muted-foreground">
                      Action
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {payslipHistory.map((slip) => (
                    <tr
                      key={`${slip.month}-${slip.year}`}
                      className="border-b border-border last:border-0 hover:bg-muted/30 transition-colors"
                    >
                      <td className="p-4">
                        <p className="font-medium">{slip.month} {slip.year}</p>
                        {slip.paidDate && (
                          <p className="text-xs text-muted-foreground">
                            Paid on {slip.paidDate}
                          </p>
                        )}
                      </td>
                      <td className="p-4 text-right text-sm">
                        {formatCurrency(slip.gross)}
                      </td>
                      <td className="p-4 text-right text-sm text-destructive">
                        -{formatCurrency(slip.deductions)}
                      </td>
                      <td className="p-4 text-right text-sm font-semibold">
                        {formatCurrency(slip.net)}
                      </td>
                      <td className="p-4 text-center">
                        <StatusBadge 
                          variant={slip.status === 'paid' ? 'approved' : 'pending'}
                          showIcon={false}
                        >
                          {slip.status === 'paid' ? 'Paid' : 'Pending'}
                        </StatusBadge>
                      </td>
                      <td className="p-4 text-right">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={handleDownload}
                          disabled={slip.status !== 'paid'}
                        >
                          <Download className="h-4 w-4" />
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        {/* Bank Details */}
        <Card className="hrms-card">
          <CardHeader>
            <CardTitle className="text-lg font-display flex items-center gap-2">
              <Building2 className="h-5 w-5 text-primary" />
              Payment Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="p-4 rounded-lg bg-muted/50 space-y-3">
              {[
                { label: "Bank Name", value: bankDetails.bankName },
                { label: "Account Number", value: bankDetails.accountNumber },
                { label: "IFSC Code", value: bankDetails.ifscCode },
                { label: "Account Type", value: bankDetails.accountType },
              ].map((item) => (
                <div key={item.label} className="flex justify-between">
                  <span className="text-sm text-muted-foreground">{item.label}</span>
                  <span className="text-sm font-medium">{item.value}</span>
                </div>
              ))}
            </div>
            <p className="text-xs text-muted-foreground text-center">
              Salary is credited to this account
            </p>

            <div className="border-t border-border pt-4 mt-4">
              <h5 className="font-medium mb-3">Tax Documents</h5>
              <div className="space-y-2">
                <Button variant="outline" className="w-full justify-start" onClick={handleDownload}>
                  <FileText className="h-4 w-4 mr-2" />
                  Download Form 16 (FY 2024-25)
                </Button>
                <Button variant="outline" className="w-full justify-start" onClick={handleDownload}>
                  <FileText className="h-4 w-4 mr-2" />
                  Download Form 16 (FY 2023-24)
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
