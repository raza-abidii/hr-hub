import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { toast } from "sonner";
import { Plus, Search, Pencil, Trash2, CreditCard, TrendingUp, TrendingDown, Percent, IndianRupee } from "lucide-react";

interface PayrollComponent {
  id: string;
  name: string;
  code: string;
  type: "earning" | "deduction";
  calculationType: "fixed" | "percentage" | "formula";
  value: number;
  taxApplicable: boolean;
  statutory: boolean;
  displayOrder: number;
}

const initialComponents: PayrollComponent[] = [
  { id: "1", name: "Basic Salary", code: "BASIC", type: "earning", calculationType: "fixed", value: 0, taxApplicable: true, statutory: false, displayOrder: 1 },
  { id: "2", name: "House Rent Allowance", code: "HRA", type: "earning", calculationType: "percentage", value: 40, taxApplicable: true, statutory: false, displayOrder: 2 },
  { id: "3", name: "Special Allowance", code: "SA", type: "earning", calculationType: "fixed", value: 0, taxApplicable: true, statutory: false, displayOrder: 3 },
  { id: "4", name: "Performance Bonus", code: "BONUS", type: "earning", calculationType: "fixed", value: 0, taxApplicable: true, statutory: false, displayOrder: 4 },
  { id: "5", name: "Provident Fund (Employee)", code: "EPF", type: "deduction", calculationType: "percentage", value: 12, taxApplicable: false, statutory: true, displayOrder: 1 },
  { id: "6", name: "Professional Tax", code: "PT", type: "deduction", calculationType: "fixed", value: 200, taxApplicable: false, statutory: true, displayOrder: 2 },
  { id: "7", name: "Income Tax (TDS)", code: "TDS", type: "deduction", calculationType: "percentage", value: 0, taxApplicable: false, statutory: true, displayOrder: 3 },
  { id: "8", name: "Insurance Premium", code: "INS", type: "deduction", calculationType: "fixed", value: 0, taxApplicable: false, statutory: false, displayOrder: 4 },
];

const calculationTypes = ["fixed", "percentage", "formula"];

export default function PayrollMaster() {
  const [components, setComponents] = useState(initialComponents);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingComponent, setEditingComponent] = useState<PayrollComponent | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    type: "earning" as "earning" | "deduction",
    calculationType: "fixed" as "fixed" | "percentage" | "formula",
    value: 0,
    taxApplicable: false,
    statutory: false,
    displayOrder: 1,
  });

  const earnings = components.filter((c) => c.type === "earning");
  const deductions = components.filter((c) => c.type === "deduction");

  const filteredComponents = (type: "earning" | "deduction") =>
    components
      .filter((c) => c.type === type)
      .filter(
        (c) =>
          c.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
          c.code.toLowerCase().includes(searchQuery.toLowerCase())
      )
      .sort((a, b) => a.displayOrder - b.displayOrder);

  const handleSubmit = () => {
    if (!formData.name || !formData.code) {
      toast.error("Please fill all required fields");
      return;
    }

    if (editingComponent) {
      setComponents((prev) =>
        prev.map((c) =>
          c.id === editingComponent.id ? { ...c, ...formData } : c
        )
      );
      toast.success("Component updated successfully!");
    } else {
      setComponents((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData },
      ]);
      toast.success("Component created successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (component: PayrollComponent) => {
    setEditingComponent(component);
    setFormData({
      name: component.name,
      code: component.code,
      type: component.type,
      calculationType: component.calculationType,
      value: component.value,
      taxApplicable: component.taxApplicable,
      statutory: component.statutory,
      displayOrder: component.displayOrder,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    setComponents((prev) => prev.filter((c) => c.id !== id));
    toast.success("Component deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingComponent(null);
    setFormData({
      name: "",
      code: "",
      type: "earning",
      calculationType: "fixed",
      value: 0,
      taxApplicable: false,
      statutory: false,
      displayOrder: 1,
    });
  };

  const ComponentCard = ({ component }: { component: PayrollComponent }) => (
    <Card className="hrms-card hover:shadow-md transition-shadow">
      <CardContent className="p-4">
        <div className="flex items-start justify-between">
          <div className="space-y-2">
            <div className="flex items-center gap-2">
              {component.type === "earning" ? (
                <TrendingUp className="h-5 w-5 text-success" />
              ) : (
                <TrendingDown className="h-5 w-5 text-destructive" />
              )}
              <div>
                <h3 className="font-semibold">{component.name}</h3>
                <span className="text-xs text-muted-foreground">{component.code}</span>
              </div>
            </div>
            <div className="flex items-center gap-2 text-lg font-bold">
              {component.calculationType === "percentage" ? (
                <>
                  <Percent className="h-4 w-4 text-primary" />
                  <span className="text-primary">{component.value}%</span>
                  <span className="text-xs font-normal text-muted-foreground">of Basic</span>
                </>
              ) : component.calculationType === "fixed" ? (
                <>
                  <IndianRupee className="h-4 w-4 text-primary" />
                  <span className="text-primary">
                    {component.value > 0 ? component.value.toLocaleString() : "Variable"}
                  </span>
                </>
              ) : (
                <span className="text-sm text-muted-foreground">Formula Based</span>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              {component.statutory && (
                <span className="px-2 py-1 rounded-full bg-warning/10 text-warning text-xs font-medium">
                  Statutory
                </span>
              )}
              {component.taxApplicable && (
                <span className="px-2 py-1 rounded-full bg-info/10 text-info text-xs font-medium">
                  Taxable
                </span>
              )}
              <span className="px-2 py-1 rounded-full bg-muted text-muted-foreground text-xs font-medium capitalize">
                {component.calculationType}
              </span>
            </div>
          </div>
          <div className="flex gap-1">
            <Button variant="ghost" size="icon" onClick={() => handleEdit(component)}>
              <Pencil className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="text-destructive"
              onClick={() => handleDelete(component.id)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Payroll Master"
          subtitle="Configure salary components and payroll settings"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Component
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search components..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <Tabs defaultValue="earnings" className="space-y-4">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="earnings" className="gap-2">
            <TrendingUp className="h-4 w-4" />
            Earnings ({earnings.length})
          </TabsTrigger>
          <TabsTrigger value="deductions" className="gap-2">
            <TrendingDown className="h-4 w-4" />
            Deductions ({deductions.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="earnings" className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {filteredComponents("earning").map((component) => (
              <ComponentCard key={component.id} component={component} />
            ))}
          </div>
        </TabsContent>

        <TabsContent value="deductions" className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {filteredComponents("deduction").map((component) => (
              <ComponentCard key={component.id} component={component} />
            ))}
          </div>
        </TabsContent>
      </Tabs>

      <Dialog open={showDialog} onOpenChange={handleCloseDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingComponent ? "Edit Component" : "Add New Component"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Component Name *</Label>
                <Input
                  placeholder="e.g., Basic Salary"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Code *</Label>
                <Input
                  placeholder="e.g., BASIC"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  maxLength={6}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Type</Label>
                <Select
                  value={formData.type}
                  onValueChange={(v: "earning" | "deduction") => setFormData({ ...formData, type: v })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="earning">Earning</SelectItem>
                    <SelectItem value="deduction">Deduction</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Calculation Type</Label>
                <Select
                  value={formData.calculationType}
                  onValueChange={(v: "fixed" | "percentage" | "formula") =>
                    setFormData({ ...formData, calculationType: v })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="fixed">Fixed Amount</SelectItem>
                    <SelectItem value="percentage">Percentage</SelectItem>
                    <SelectItem value="formula">Formula</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>
                  {formData.calculationType === "percentage" ? "Percentage (%)" : "Amount (₹)"}
                </Label>
                <Input
                  type="number"
                  value={formData.value}
                  onChange={(e) => setFormData({ ...formData, value: parseFloat(e.target.value) || 0 })}
                />
              </div>
              <div className="space-y-2">
                <Label>Display Order</Label>
                <Input
                  type="number"
                  value={formData.displayOrder}
                  onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 1 })}
                />
              </div>
            </div>
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label>Tax Applicable</Label>
                <Switch
                  checked={formData.taxApplicable}
                  onCheckedChange={(v) => setFormData({ ...formData, taxApplicable: v })}
                />
              </div>
              <div className="flex items-center justify-between">
                <Label>Statutory (PF, ESI, PT)</Label>
                <Switch
                  checked={formData.statutory}
                  onCheckedChange={(v) => setFormData({ ...formData, statutory: v })}
                />
              </div>
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingComponent ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
