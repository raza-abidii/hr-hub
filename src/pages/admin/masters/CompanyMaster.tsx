import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { toast } from "sonner";
import { Plus, Search, Pencil, Trash2, Building2, MapPin, Phone, Mail, Globe, Users } from "lucide-react";

interface Company {
  id: string;
  name: string;
  shortName: string;
  registrationNo: string;
  taxId: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
  country: string;
  phone: string;
  email: string;
  website: string;
  employeeCount: number;
}

const initialCompanies: Company[] = [
  {
    id: "1",
    name: "SFON Technologies Pvt Ltd",
    shortName: "SFON",
    registrationNo: "U72900DL2020PTC123456",
    taxId: "ABCDE1234F",
    address: "123 Tech Park, Sector 62",
    city: "Noida",
    state: "Uttar Pradesh",
    pinCode: "201301",
    country: "India",
    phone: "+91 120 4567890",
    email: "info@sfon.com",
    website: "www.sfon.com",
    employeeCount: 50,
  },
];

export default function CompanyMaster() {
  const [companies, setCompanies] = useState(initialCompanies);
  const [searchQuery, setSearchQuery] = useState("");
  const [showDialog, setShowDialog] = useState(false);
  const [editingCompany, setEditingCompany] = useState<Company | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    shortName: "",
    registrationNo: "",
    taxId: "",
    address: "",
    city: "",
    state: "",
    pinCode: "",
    country: "India",
    phone: "",
    email: "",
    website: "",
  });

  const filteredCompanies = companies.filter(
    (c) =>
      c.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      c.shortName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = () => {
    if (!formData.name || !formData.shortName || !formData.email) {
      toast.error("Please fill all required fields");
      return;
    }

    if (editingCompany) {
      setCompanies((prev) =>
        prev.map((c) =>
          c.id === editingCompany.id ? { ...c, ...formData } : c
        )
      );
      toast.success("Company updated successfully!");
    } else {
      setCompanies((prev) => [
        ...prev,
        { id: Date.now().toString(), ...formData, employeeCount: 0 },
      ]);
      toast.success("Company created successfully!");
    }
    handleCloseDialog();
  };

  const handleEdit = (company: Company) => {
    setEditingCompany(company);
    setFormData({
      name: company.name,
      shortName: company.shortName,
      registrationNo: company.registrationNo,
      taxId: company.taxId,
      address: company.address,
      city: company.city,
      state: company.state,
      pinCode: company.pinCode,
      country: company.country,
      phone: company.phone,
      email: company.email,
      website: company.website,
    });
    setShowDialog(true);
  };

  const handleDelete = (id: string) => {
    const company = companies.find((c) => c.id === id);
    if (company?.employeeCount && company.employeeCount > 0) {
      toast.error("Cannot delete company with assigned employees");
      return;
    }
    setCompanies((prev) => prev.filter((c) => c.id !== id));
    toast.success("Company deleted successfully!");
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingCompany(null);
    setFormData({
      name: "",
      shortName: "",
      registrationNo: "",
      taxId: "",
      address: "",
      city: "",
      state: "",
      pinCode: "",
      country: "India",
      phone: "",
      email: "",
      website: "",
    });
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <PageHeader
          title="Company Master"
          subtitle="Manage company/branch information"
        />
        <Button className="gradient-primary text-primary-foreground" onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Company
        </Button>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search companies..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="grid grid-cols-1 gap-4">
        {filteredCompanies.map((company) => (
          <Card key={company.id} className="hrms-card hover:shadow-md transition-shadow">
            <CardContent className="p-6">
              <div className="flex flex-col lg:flex-row lg:items-start justify-between gap-4">
                <div className="flex items-start gap-4">
                  <div className="w-16 h-16 rounded-xl gradient-primary flex items-center justify-center flex-shrink-0">
                    <Building2 className="h-8 w-8 text-primary-foreground" />
                  </div>
                  <div className="space-y-2">
                    <div>
                      <h3 className="font-semibold text-lg">{company.name}</h3>
                      <span className="text-sm text-muted-foreground">{company.shortName}</span>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-1 text-sm">
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <MapPin className="h-4 w-4" />
                        <span>{company.city}, {company.state}</span>
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Phone className="h-4 w-4" />
                        <span>{company.phone}</span>
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Mail className="h-4 w-4" />
                        <span>{company.email}</span>
                      </div>
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Globe className="h-4 w-4" />
                        <span>{company.website}</span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 pt-2">
                      <Users className="h-4 w-4 text-primary" />
                      <span className="text-sm font-medium">{company.employeeCount} employees</span>
                    </div>
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" size="sm" onClick={() => handleEdit(company)}>
                    <Pencil className="h-4 w-4 mr-1" /> Edit
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    className="text-destructive border-destructive hover:bg-destructive/10"
                    onClick={() => handleDelete(company.id)}
                  >
                    <Trash2 className="h-4 w-4 mr-1" /> Delete
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog open={showDialog} onOpenChange={handleCloseDialog}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editingCompany ? "Edit Company" : "Add New Company"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Company Name *</Label>
                <Input
                  placeholder="e.g., SFON Technologies Pvt Ltd"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Short Name *</Label>
                <Input
                  placeholder="e.g., SFON"
                  value={formData.shortName}
                  onChange={(e) => setFormData({ ...formData, shortName: e.target.value })}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Registration Number</Label>
                <Input
                  placeholder="Corporate registration number"
                  value={formData.registrationNo}
                  onChange={(e) => setFormData({ ...formData, registrationNo: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Tax ID / PAN</Label>
                <Input
                  placeholder="e.g., ABCDE1234F"
                  value={formData.taxId}
                  onChange={(e) => setFormData({ ...formData, taxId: e.target.value })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Address</Label>
              <Textarea
                placeholder="Complete address"
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                rows={2}
              />
            </div>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="space-y-2">
                <Label>City</Label>
                <Input
                  placeholder="City"
                  value={formData.city}
                  onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>State</Label>
                <Input
                  placeholder="State"
                  value={formData.state}
                  onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>PIN Code</Label>
                <Input
                  placeholder="PIN Code"
                  value={formData.pinCode}
                  onChange={(e) => setFormData({ ...formData, pinCode: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Country</Label>
                <Input
                  placeholder="Country"
                  value={formData.country}
                  onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Phone</Label>
                <Input
                  placeholder="+91 XXX XXXXXXX"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                />
              </div>
              <div className="space-y-2">
                <Label>Email *</Label>
                <Input
                  type="email"
                  placeholder="info@company.com"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Website</Label>
              <Input
                placeholder="www.company.com"
                value={formData.website}
                onChange={(e) => setFormData({ ...formData, website: e.target.value })}
              />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="flex-1" onClick={handleCloseDialog}>
              Cancel
            </Button>
            <Button className="flex-1 gradient-primary text-primary-foreground" onClick={handleSubmit}>
              {editingCompany ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
