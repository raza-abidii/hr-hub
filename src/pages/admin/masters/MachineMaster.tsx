import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent } from "@/components/ui/card";
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
import { toast } from "sonner";
import { Plus, Search, Pencil, Trash2, Monitor, MapPin, Wifi, WifiOff, RefreshCw } from "lucide-react";

interface Machine {
  id: string;
  name: string;
  machineId: string;
  location: string;
  ipAddress: string;
  port: number;
  deviceType: string;
  status: boolean;
  lastSync: string;
}

const initialMachines: Machine[] = [
  { id: "1", name: "Main Gate 1", machineId: "M001", location: "Building A Entrance", ipAddress: "192.168.1.101", port: 4370, deviceType: "Biometric", status: true, lastSync: "Jan 7, 2026 09:15 AM" },
  { id: "2", name: "Floor 2 Entry", machineId: "M002", location: "Building A Floor 2", ipAddress: "192.168.1.102", port: 4370, deviceType: "RFID", status: true, lastSync: "Jan 7, 2026 09:12 AM" },
  { id: "3", name: "Cafeteria", machineId: "M003", location: "Cafeteria Entrance", ipAddress: "192.168.1.103", port: 4370, deviceType: "Face Recognition", status: false, lastSync: "Jan 6, 2026 06:30 PM" },
];

const deviceTypes = ["Biometric", "RFID", "Face Recognition", "QR Scanner"];

export default function MachineMaster() {
  const [machines, setMachines] = useState(initialMachines);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingMachine, setEditingMachine] = useState<Machine | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    machineId: "",
    location: "",
    ipAddress: "",
    port: 4370,
    deviceType: "",
    status: true,
  });

  const filteredMachines = machines.filter(
    (m) =>
      m.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      m.machineId.toLowerCase().includes(searchQuery.toLowerCase()) ||
      m.location.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = () => {
    if (!formData.name || !formData.machineId || !formData.ipAddress || !formData.deviceType) {
      toast.error("Please fill all required fields");
      return;
    }

    const now = new Date().toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });

    if (editingMachine) {
      setMachines((prev) =>
        prev.map((m) =>
          m.id === editingMachine.id ? { ...m, ...formData } : m
        )
      );
      toast.success("Machine updated successfully!");
    } else {
      setMachines((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData, lastSync: now },
      ]);
      toast.success("Machine added successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (machine: Machine) => {
    setEditingMachine(machine);
    setFormData({
      name: machine.name,
      machineId: machine.machineId,
      location: machine.location,
      ipAddress: machine.ipAddress,
      port: machine.port,
      deviceType: machine.deviceType,
      status: machine.status,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    setMachines((prev) => prev.filter((m) => m.id !== id));
    toast.success("Machine deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingMachine(null);
    setFormData({
      name: "",
      machineId: "",
      location: "",
      ipAddress: "",
      port: 4370,
      deviceType: "",
      status: true,
    });
  };

  const handleSync = (machine: Machine) => {
    toast.info(`Syncing ${machine.name}...`);
    setTimeout(() => {
      const now = new Date().toLocaleString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      });
      setMachines((prev) =>
        prev.map((m) => (m.id === machine.id ? { ...m, lastSync: now } : m))
      );
      toast.success(`${machine.name} synced successfully!`);
    }, 1500);
  };

  const handleTestConnection = (machine: Machine) => {
    toast.info(`Testing connection to ${machine.name}...`);
    setTimeout(() => {
      if (machine.status) {
        toast.success(`Connection to ${machine.name} successful!`);
      } else {
        toast.error(`Failed to connect to ${machine.name}`);
      }
    }, 1000);
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Machine Master"
          subtitle="Manage biometric devices and attendance machines"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Machine
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search machines..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredMachines.map((machine) => (
          <Card key={machine.id} className="hrms-card hover:shadow-md transition-shadow">
            <CardContent className="p-4">
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                    machine.status ? "bg-success/10" : "bg-destructive/10"
                  }`}>
                    <Monitor className={`h-5 w-5 ${
                      machine.status ? "text-success" : "text-destructive"
                    }`} />
                  </div>
                  <div>
                    <h3 className="font-semibold">{machine.name}</h3>
                    <span className="text-xs text-muted-foreground">{machine.machineId}</span>
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  {machine.status ? (
                    <Wifi className="h-4 w-4 text-success" />
                  ) : (
                    <WifiOff className="h-4 w-4 text-destructive" />
                  )}
                </div>
              </div>
              
              <div className="space-y-2 text-sm mb-4">
                <div className="flex items-center gap-2 text-muted-foreground">
                  <MapPin className="h-4 w-4" />
                  <span>{machine.location}</span>
                </div>
                <div className="flex items-center gap-2 text-muted-foreground">
                  <span className="font-mono text-xs">{machine.ipAddress}:{machine.port}</span>
                </div>
                <div className="flex flex-wrap gap-2">
                  <span className="px-2 py-1 rounded-full bg-primary/10 text-primary text-xs font-medium">
                    {machine.deviceType}
                  </span>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                    machine.status ? "bg-success/10 text-success" : "bg-destructive/10 text-destructive"
                  }`}>
                    {machine.status ? "Online" : "Offline"}
                  </span>
                </div>
                <p className="text-xs text-muted-foreground">
                  Last sync: {machine.lastSync}
                </p>
              </div>

              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="flex-1"
                  onClick={() => handleSync(machine)}
                >
                  <RefreshCw className="h-3 w-3 mr-1" /> Sync
                </Button>
                <Button variant="ghost" size="icon" onClick={() => handleEdit(machine)}>
                  <Pencil className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="text-destructive"
                  onClick={() => handleDelete(machine.id)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog open={showDialog} onOpenChange={handleCloseDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingMachine ? "Edit Machine" : "Add New Machine"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Machine Name *</Label>
                <Input
                  placeholder="e.g., Main Gate 1"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Machine ID *</Label>
                <Input
                  placeholder="e.g., M001"
                  value={formData.machineId}
                  onChange={(e) => setFormData({ ...formData, machineId: e.target.value })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Location</Label>
              <Input
                placeholder="Physical location"
                value={formData.location}
                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>IP Address *</Label>
                <Input
                  placeholder="192.168.1.100"
                  value={formData.ipAddress}
                  onChange={(e) => setFormData({ ...formData, ipAddress: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Port</Label>
                <Input
                  type="number"
                  value={formData.port}
                  onChange={(e) => setFormData({ ...formData, port: parseInt(e.target.value) || 4370 })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Device Type *</Label>
              <Select value={formData.deviceType} onValueChange={(v) => setFormData({ ...formData, deviceType: v })}>
                <SelectTrigger>
                  <SelectValue placeholder="Select device type" />
                </SelectTrigger>
                <SelectContent>
                  {deviceTypes.map((type) => (
                    <SelectItem key={type} value={type}>{type}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center justify-between">
              <Label>Active</Label>
              <Switch
                checked={formData.status}
                onCheckedChange={(v) => setFormData({ ...formData, status: v })}
              />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingMachine ? "Update" : "Add"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
