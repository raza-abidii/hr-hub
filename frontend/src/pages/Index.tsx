import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/lib/auth-context";

const Index = () => {
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();

  useEffect(() => {
    if (isAuthenticated && user) {
      if (user.role === "admin") {
        navigate("/admin");
      } else {
        navigate("/employee");
      }
    } else {
      navigate("/login");
    }
  }, [isAuthenticated, user, navigate]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="text-center">
        <div className="h-8 w-8 border-4 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" />
        <p className="text-muted-foreground mt-4">Loading...</p>
      </div>
    </div>
  );
};

export default Index;
