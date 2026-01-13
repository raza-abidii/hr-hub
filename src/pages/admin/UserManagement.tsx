import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
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
  DialogFooter,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { StatusBadge } from "@/components/ui/status-badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { toast } from "sonner";
import {
  Plus,
  Search,
  Edit,
  Trash2,
  Shield,
  ShieldCheck,
  User,
} from "lucide-react";

interface UserData {
  id: number;
  name: string;
  email: string;
  role: "admin" | "employee" | "manager";
  department: string;
  status: "active" | "inactive";
  lastLogin: string;
}

const mockUsers: UserData[] = [
  { id: 1, name: "John Smith", email: "john.smith@company.com", role: "admin", department: "IT", status: "active", lastLogin: "2024-01-07 09:30 AM" },
  { id: 2, name: "Sarah Wilson", email: "sarah.wilson@company.com", role: "manager", department: "HR", status: "active", lastLogin: "2024-01-07 08:45 AM" },
  { id: 3, name: "Mike Johnson", email: "mike.johnson@company.com", role: "employee", department: "Finance", status: "active", lastLogin: "2024-01-06 04:20 PM" },
  { id: 4, name: "Emily Davis", email: "emily.davis@company.com", role: "employee", department: "Marketing", status: "inactive", lastLogin: "2024-01-02 11:00 AM" },
  { id: 5, name: "Tom Brown", email: "tom.brown@company.com", role: "manager", department: "Sales", status: "active", lastLogin: "2024-01-07 10:15 AM" },
  { id: 6, name: "Lisa Anderson", email: "lisa.anderson@company.com", role: "employee", department: "IT", status: "active", lastLogin: "2024-01-07 07:50 AM" },
];

const roles = ["admin", "manager", "employee"];
const departments = ["IT", "HR", "Finance", "Marketing", "Sales", "Operations"];

export default function UserManagement() {
  const [users, setUsers] = useState<UserData[]>(mockUsers);
  const [searchQuery, setSearchQuery] = useState("");
  const [roleFilter, setRoleFilter] = useState("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UserData | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    role: "",
    department: "",
    password: "",
  });

  const filteredUsers = users.filter((user) => {
    const matchesSearch =
      user.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      user.email.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesRole = roleFilter === "all" || user.role === roleFilter;
    return matchesSearch && matchesRole;
  });

  const openAddDialog = () => {
    setEditingUser(null);
    setFormData({ name: "", email: "", role: "", department: "", password: "" });
    setIsDialogOpen(true);
  };

  const openEditDialog = (user: UserData) => {
    setEditingUser(user);
    setFormData({
      name: user.name,
      email: user.email,
      role: user.role,
      department: user.department,
      password: "",
    });
    setIsDialogOpen(true);
  };

  const handleSubmit = () => {
    if (!formData.name || !formData.email || !formData.role || !formData.department) {
      toast.error("Please fill all required fields");
      return;
    }

    if (!editingUser && !formData.password) {
      toast.error("Password is required for new users");
      return;
    }

    if (editingUser) {
      setUsers(users.map((u) =>
        u.id === editingUser.id
          ? { ...u, name: formData.name, email: formData.email, role: formData.role as UserData["role"], department: formData.department }
          : u
      ));
      toast.success("User updated successfully");
    } else {
      const newUser: UserData = {
        id: Date.now(),
        name: formData.name,
        email: formData.email,
        role: formData.role as UserData["role"],
        department: formData.department,
        status: "active",
        lastLogin: "-",
      };
      setUsers([...users, newUser]);
      toast.success("User created successfully");
    }
    setIsDialogOpen(false);
  };

  const handleDelete = (user: UserData) => {
    if (user.role === "admin") {
      toast.error("Cannot delete admin users");
      return;
    }
    setUsers(users.filter((u) => u.id !== user.id));
    toast.success("User deleted successfully");
  };

  const toggleStatus = (user: UserData) => {
    setUsers(users.map((u) =>
      u.id === user.id
        ? { ...u, status: u.status === "active" ? "inactive" : "active" }
        : u
    ));
    toast.success(`User ${user.status === "active" ? "deactivated" : "activated"}`);
  };

  const getRoleIcon = (role: string) => {
    switch (role) {
      case "admin":
        return <ShieldCheck className="h-4 w-4 text-destructive" />;
      case "manager":
        return <Shield className="h-4 w-4 text-warning" />;
      default:
        return <User className="h-4 w-4 text-muted-foreground" />;
    }
  };

  const adminCount = users.filter((u) => u.role === "admin").length;
  const managerCount = users.filter((u) => u.role === "manager").length;
  const employeeCount = users.filter((u) => u.role === "employee").length;
  const activeCount = users.filter((u) => u.status === "active").length;

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="User Management"
        subtitle="Manage user accounts and access permissions"
        action={
          <Button onClick={openAddDialog} className="gradient-primary gap-2">
            <Plus className="h-4 w-4" />
            Add User
          </Button>
        }
      />

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card className="hrms-card">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-destructive/10 flex items-center justify-center">
                <ShieldCheck className="h-5 w-5 text-destructive" />
              </div>
              <div>
                <p className="text-2xl font-bold">{adminCount}</p>
                <p className="text-xs text-muted-foreground">Admins</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="hrms-card">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-warning/10 flex items-center justify-center">
                <Shield className="h-5 w-5 text-warning" />
              </div>
              <div>
                <p className="text-2xl font-bold">{managerCount}</p>
                <p className="text-xs text-muted-foreground">Managers</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="hrms-card">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                <User className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{employeeCount}</p>
                <p className="text-xs text-muted-foreground">Employees</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="hrms-card">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-success/10 flex items-center justify-center">
                <User className="h-5 w-5 text-success" />
              </div>
              <div>
                <p className="text-2xl font-bold">{activeCount}</p>
                <p className="text-xs text-muted-foreground">Active Users</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="hrms-card">
        <CardContent className="p-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search users..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={roleFilter} onValueChange={setRoleFilter}>
              <SelectTrigger className="w-full sm:w-40">
                <SelectValue placeholder="Filter by role" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Roles</SelectItem>
                {roles.map((role) => (
                  <SelectItem key={role} value={role}>
                    {role.charAt(0).toUpperCase() + role.slice(1)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Users Table */}
      <Card className="hrms-card">
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>User</TableHead>
                <TableHead>Role</TableHead>
                <TableHead>Department</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Last Login</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredUsers.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar className="h-9 w-9">
                        <AvatarFallback className="bg-primary/10 text-primary text-sm">
                          {user.name.split(" ").map((n) => n[0]).join("")}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-medium text-sm">{user.name}</p>
                        <p className="text-xs text-muted-foreground">{user.email}</p>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      {getRoleIcon(user.role)}
                      <span className="capitalize text-sm">{user.role}</span>
                    </div>
                  </TableCell>
                  <TableCell className="text-sm">{user.department}</TableCell>
                  <TableCell>
                    <button onClick={() => toggleStatus(user)}>
                      <StatusBadge variant={user.status === "active" ? "present" : "absent"} />
                    </button>
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground">{user.lastLogin}</TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => openEditDialog(user)}
                      >
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDelete(user)}
                        disabled={user.role === "admin"}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Add/Edit Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingUser ? "Edit User" : "Add New User"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Full Name *</Label>
              <Input
                placeholder="Enter full name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label>Email *</Label>
              <Input
                type="email"
                placeholder="email@company.com"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Role *</Label>
                <Select
                  value={formData.role}
                  onValueChange={(v) => setFormData({ ...formData, role: v })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select role" />
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (
                      <SelectItem key={role} value={role}>
                        {role.charAt(0).toUpperCase() + role.slice(1)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Department *</Label>
                <Select
                  value={formData.department}
                  onValueChange={(v) => setFormData({ ...formData, department: v })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select department" />
                  </SelectTrigger>
                  <SelectContent>
                    {departments.map((dept) => (
                      <SelectItem key={dept} value={dept}>
                        {dept}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2">
              <Label>{editingUser ? "New Password (leave blank to keep current)" : "Password *"}</Label>
              <Input
                type="password"
                placeholder="Enter password"
                value={formData.password}
                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSubmit} className="gradient-primary">
              {editingUser ? "Update" : "Create"} User
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
