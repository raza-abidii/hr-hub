import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  User,
  Briefcase,
  Phone,
  Building2,
  FileText,
  Settings,
  Mail,
  MapPin,
  Calendar,
  Upload,
  Lock,
  Bell,
  Eye,
  EyeOff,
  Check,
  Shield,
} from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { useToast } from "@/hooks/use-toast";
import { StatusBadge } from "@/components/ui/status-badge";

const mockProfile = {
  personal: {
    firstName: "Sarah",
    lastName: "Wilson",
    email: "sarah.wilson@sfon.com",
    phone: "+1 (555) 123-4567",
    dateOfBirth: "1990-05-15",
    gender: "Female",
    address: "123 Main Street, Apt 4B",
    city: "New York",
    state: "NY",
    zipCode: "10001",
  },
  employment: {
    employeeId: "EMP001",
    department: "Information Technology",
    designation: "Senior Developer",
    joiningDate: "Mar 15, 2023",
    employmentType: "Full-time",
    workLocation: "New York Office",
    reportingTo: "John Smith",
    shift: "General Shift (9 AM - 6 PM)",
    status: "Active",
  },
  emergency: {
    contactName: "John Wilson",
    relationship: "Spouse",
    phone: "+1 (555) 987-6543",
    email: "john.wilson@email.com",
  },
  bank: {
    bankName: "State Bank of India",
    accountNumber: "****7890",
    ifscCode: "SBIN0001234",
    accountType: "Savings",
    panNumber: "****E1234F",
  },
  documents: [
    { type: "Aadhaar Card", status: "verified" as const, uploadDate: "Mar 15, 2023" },
    { type: "PAN Card", status: "verified" as const, uploadDate: "Mar 15, 2023" },
    { type: "Offer Letter", status: "verified" as const, uploadDate: "Mar 15, 2023" },
    { type: "Degree Certificate", status: "pending" as const, uploadDate: null },
  ],
  settings: {
    emailNotifications: true,
    pushNotifications: true,
    smsAlerts: false,
    showPhoneInDirectory: true,
    showEmailInDirectory: true,
  },
};

export default function Profile() {
  const { user } = useAuth();
  const { toast } = useToast();
  const [isEditing, setIsEditing] = useState(false);
  const [isPasswordDialogOpen, setIsPasswordDialogOpen] = useState(false);
  const [profile, setProfile] = useState(mockProfile);
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false,
  });
  const [settings, setSettings] = useState(mockProfile.settings);

  const handleSave = () => {
    toast({
      title: "Success!",
      description: "Profile updated successfully",
    });
    setIsEditing(false);
  };

  const handlePasswordChange = () => {
    if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
      toast({
        title: "Error",
        description: "Please fill all password fields",
        variant: "destructive",
      });
      return;
    }
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      toast({
        title: "Error",
        description: "New passwords do not match",
        variant: "destructive",
      });
      return;
    }
    if (passwordForm.newPassword.length < 8) {
      toast({
        title: "Error",
        description: "Password must be at least 8 characters",
        variant: "destructive",
      });
      return;
    }

    toast({
      title: "Success!",
      description: "Password changed successfully. Please login again.",
    });
    setIsPasswordDialogOpen(false);
    setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
  };

  const handleSettingChange = (key: keyof typeof settings, value: boolean) => {
    setSettings((prev) => ({ ...prev, [key]: value }));
    toast({
      title: "Preference Updated",
      description: `Setting has been ${value ? 'enabled' : 'disabled'}`,
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <PageHeader
        title="Profile"
        subtitle="View and manage your personal information"
        action={
          isEditing ? (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setIsEditing(false)}>
                Cancel
              </Button>
              <Button className="gradient-primary" onClick={handleSave}>
                <Check className="h-4 w-4 mr-2" />
                Save Changes
              </Button>
            </div>
          ) : (
            <Button variant="outline" onClick={() => setIsEditing(true)}>
              Edit Profile
            </Button>
          )
        }
      />

      {/* Profile Header Card */}
      <Card className="hrms-card overflow-hidden">
        <div className="h-24 gradient-primary" />
        <CardContent className="relative pt-0">
          <div className="flex flex-col sm:flex-row items-center sm:items-end gap-4 -mt-12">
            <Avatar className="h-24 w-24 border-4 border-card shadow-lg">
              <AvatarFallback className="gradient-primary text-primary-foreground text-2xl font-bold">
                {profile.personal.firstName.charAt(0)}
                {profile.personal.lastName.charAt(0)}
              </AvatarFallback>
            </Avatar>
            <div className="text-center sm:text-left pb-2">
              <h2 className="text-2xl font-display font-bold">
                {profile.personal.firstName} {profile.personal.lastName}
              </h2>
              <p className="text-muted-foreground">
                {profile.employment.employeeId} • {profile.employment.designation}
              </p>
              <div className="flex items-center justify-center sm:justify-start gap-2 mt-2">
                <span className="px-2 py-0.5 bg-primary/10 text-primary text-xs font-medium rounded">
                  {profile.employment.department}
                </span>
                <StatusBadge variant="active" showIcon={false} />
              </div>
            </div>
            {isEditing && (
              <Button variant="outline" size="sm" className="sm:ml-auto">
                <Upload className="h-4 w-4 mr-2" />
                Change Photo
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="personal" className="space-y-6">
        <TabsList className="grid w-full grid-cols-3 lg:grid-cols-6 lg:w-auto lg:inline-grid">
          <TabsTrigger value="personal">Personal</TabsTrigger>
          <TabsTrigger value="employment">Employment</TabsTrigger>
          <TabsTrigger value="emergency">Emergency</TabsTrigger>
          <TabsTrigger value="bank">Bank</TabsTrigger>
          <TabsTrigger value="documents">Documents</TabsTrigger>
          <TabsTrigger value="settings">Settings</TabsTrigger>
        </TabsList>

        {/* Personal Information Tab */}
        <TabsContent value="personal">
          <Card className="hrms-card">
            <CardHeader>
              <CardTitle className="text-lg font-display flex items-center gap-2">
                <User className="h-5 w-5 text-primary" />
                Personal Information
              </CardTitle>
            </CardHeader>
            <CardContent className="grid md:grid-cols-2 gap-4">
              {[
                { label: "First Name", value: profile.personal.firstName, icon: User },
                { label: "Last Name", value: profile.personal.lastName, icon: User },
                { label: "Email", value: profile.personal.email, icon: Mail },
                { label: "Phone", value: profile.personal.phone, icon: Phone },
                { label: "Date of Birth", value: profile.personal.dateOfBirth, icon: Calendar },
                { label: "Gender", value: profile.personal.gender, icon: User },
                { label: "Address", value: profile.personal.address, icon: MapPin, colSpan: 2 },
                { label: "City", value: profile.personal.city, icon: MapPin },
                { label: "State", value: profile.personal.state, icon: MapPin },
                { label: "ZIP Code", value: profile.personal.zipCode, icon: MapPin },
              ].map((field) => (
                <div
                  key={field.label}
                  className={field.colSpan ? "md:col-span-2" : ""}
                >
                  <Label className="text-muted-foreground text-sm">{field.label}</Label>
                  {isEditing ? (
                    <Input defaultValue={field.value} className="mt-1" />
                  ) : (
                    <p className="font-medium mt-1">{field.value}</p>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Employment Tab */}
        <TabsContent value="employment">
          <Card className="hrms-card">
            <CardHeader>
              <CardTitle className="text-lg font-display flex items-center gap-2">
                <Briefcase className="h-5 w-5 text-primary" />
                Employment Information
              </CardTitle>
            </CardHeader>
            <CardContent className="grid md:grid-cols-2 gap-4">
              {[
                { label: "Employee ID", value: profile.employment.employeeId },
                { label: "Department", value: profile.employment.department },
                { label: "Designation", value: profile.employment.designation },
                { label: "Joining Date", value: profile.employment.joiningDate },
                { label: "Employment Type", value: profile.employment.employmentType },
                { label: "Work Location", value: profile.employment.workLocation },
                { label: "Reporting Manager", value: profile.employment.reportingTo },
                { label: "Shift", value: profile.employment.shift },
              ].map((field) => (
                <div key={field.label}>
                  <Label className="text-muted-foreground text-sm">{field.label}</Label>
                  <p className="font-medium mt-1">{field.value}</p>
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Emergency Contact Tab */}
        <TabsContent value="emergency">
          <Card className="hrms-card">
            <CardHeader>
              <CardTitle className="text-lg font-display flex items-center gap-2">
                <Phone className="h-5 w-5 text-primary" />
                Emergency Contact
              </CardTitle>
            </CardHeader>
            <CardContent className="grid md:grid-cols-2 gap-4">
              {[
                { label: "Contact Name", value: profile.emergency.contactName },
                { label: "Relationship", value: profile.emergency.relationship },
                { label: "Phone", value: profile.emergency.phone },
                { label: "Email", value: profile.emergency.email },
              ].map((field) => (
                <div key={field.label}>
                  <Label className="text-muted-foreground text-sm">{field.label}</Label>
                  {isEditing ? (
                    <Input defaultValue={field.value} className="mt-1" />
                  ) : (
                    <p className="font-medium mt-1">{field.value}</p>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Bank Details Tab */}
        <TabsContent value="bank">
          <Card className="hrms-card">
            <CardHeader>
              <CardTitle className="text-lg font-display flex items-center gap-2">
                <Building2 className="h-5 w-5 text-primary" />
                Bank & Document Details
              </CardTitle>
            </CardHeader>
            <CardContent className="grid md:grid-cols-2 gap-4">
              {[
                { label: "Bank Name", value: profile.bank.bankName },
                { label: "Account Number", value: profile.bank.accountNumber },
                { label: "IFSC Code", value: profile.bank.ifscCode },
                { label: "Account Type", value: profile.bank.accountType },
                { label: "PAN Number", value: profile.bank.panNumber },
              ].map((field) => (
                <div key={field.label}>
                  <Label className="text-muted-foreground text-sm">{field.label}</Label>
                  <p className="font-medium mt-1">{field.value}</p>
                </div>
              ))}
              <div className="md:col-span-2">
                <Button variant="outline" className="mt-2">
                  <Shield className="h-4 w-4 mr-2" />
                  Request Bank Details Change
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Documents Tab */}
        <TabsContent value="documents">
          <Card className="hrms-card">
            <CardHeader>
              <CardTitle className="text-lg font-display flex items-center gap-2">
                <FileText className="h-5 w-5 text-primary" />
                Documents
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {profile.documents.map((doc) => (
                  <div
                    key={doc.type}
                    className="flex items-center justify-between p-3 rounded-lg bg-muted/50"
                  >
                    <div className="flex items-center gap-3">
                      <FileText className="h-5 w-5 text-primary" />
                      <div>
                        <p className="font-medium">{doc.type}</p>
                        {doc.uploadDate && (
                          <p className="text-xs text-muted-foreground">
                            Uploaded on {doc.uploadDate}
                          </p>
                        )}
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <StatusBadge
                        variant={doc.status === 'verified' ? 'approved' : 'pending'}
                        showIcon={false}
                      >
                        {doc.status === 'verified' ? 'Verified' : 'Pending'}
                      </StatusBadge>
                      {doc.status === 'pending' ? (
                        <Button size="sm" variant="outline">
                          <Upload className="h-4 w-4 mr-1" />
                          Upload
                        </Button>
                      ) : (
                        <Button size="sm" variant="ghost">
                          View
                        </Button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Settings Tab */}
        <TabsContent value="settings">
          <div className="space-y-6">
            {/* Password Section */}
            <Card className="hrms-card">
              <CardHeader>
                <CardTitle className="text-lg font-display flex items-center gap-2">
                  <Lock className="h-5 w-5 text-primary" />
                  Security
                </CardTitle>
              </CardHeader>
              <CardContent>
                <Dialog open={isPasswordDialogOpen} onOpenChange={setIsPasswordDialogOpen}>
                  <DialogTrigger asChild>
                    <Button variant="outline">
                      <Lock className="h-4 w-4 mr-2" />
                      Change Password
                    </Button>
                  </DialogTrigger>
                  <DialogContent>
                    <DialogHeader>
                      <DialogTitle>Change Password</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 mt-4">
                      {[
                        { key: "current", label: "Current Password", value: passwordForm.currentPassword },
                        { key: "new", label: "New Password", value: passwordForm.newPassword },
                        { key: "confirm", label: "Confirm New Password", value: passwordForm.confirmPassword },
                      ].map((field) => (
                        <div key={field.key} className="space-y-2">
                          <Label>{field.label}</Label>
                          <div className="relative">
                            <Input
                              type={showPasswords[field.key as keyof typeof showPasswords] ? "text" : "password"}
                              value={field.value}
                              onChange={(e) =>
                                setPasswordForm((prev) => ({
                                  ...prev,
                                  [field.key === "current" ? "currentPassword" : field.key === "new" ? "newPassword" : "confirmPassword"]: e.target.value,
                                }))
                              }
                              className="pr-10"
                            />
                            <button
                              type="button"
                              className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground"
                              onClick={() =>
                                setShowPasswords((prev) => ({
                                  ...prev,
                                  [field.key]: !prev[field.key as keyof typeof prev],
                                }))
                              }
                            >
                              {showPasswords[field.key as keyof typeof showPasswords] ? (
                                <EyeOff className="h-4 w-4" />
                              ) : (
                                <Eye className="h-4 w-4" />
                              )}
                            </button>
                          </div>
                        </div>
                      ))}
                      <Button className="w-full gradient-primary" onClick={handlePasswordChange}>
                        Update Password
                      </Button>
                    </div>
                  </DialogContent>
                </Dialog>
              </CardContent>
            </Card>

            {/* Notification Preferences */}
            <Card className="hrms-card">
              <CardHeader>
                <CardTitle className="text-lg font-display flex items-center gap-2">
                  <Bell className="h-5 w-5 text-primary" />
                  Notification Preferences
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {[
                  { key: "emailNotifications", label: "Email Notifications", description: "Receive updates via email" },
                  { key: "pushNotifications", label: "Push Notifications", description: "Receive browser notifications" },
                  { key: "smsAlerts", label: "SMS Alerts", description: "Receive important alerts via SMS" },
                ].map((setting) => (
                  <div key={setting.key} className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">{setting.label}</p>
                      <p className="text-sm text-muted-foreground">{setting.description}</p>
                    </div>
                    <Switch
                      checked={settings[setting.key as keyof typeof settings]}
                      onCheckedChange={(checked) =>
                        handleSettingChange(setting.key as keyof typeof settings, checked)
                      }
                    />
                  </div>
                ))}
              </CardContent>
            </Card>

            {/* Privacy Settings */}
            <Card className="hrms-card">
              <CardHeader>
                <CardTitle className="text-lg font-display flex items-center gap-2">
                  <Settings className="h-5 w-5 text-primary" />
                  Privacy Settings
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {[
                  { key: "showPhoneInDirectory", label: "Show Phone in Directory", description: "Let colleagues see your phone number" },
                  { key: "showEmailInDirectory", label: "Show Email in Directory", description: "Let colleagues see your email address" },
                ].map((setting) => (
                  <div key={setting.key} className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">{setting.label}</p>
                      <p className="text-sm text-muted-foreground">{setting.description}</p>
                    </div>
                    <Switch
                      checked={settings[setting.key as keyof typeof settings]}
                      onCheckedChange={(checked) =>
                        handleSettingChange(setting.key as keyof typeof settings, checked)
                      }
                    />
                  </div>
                ))}
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
