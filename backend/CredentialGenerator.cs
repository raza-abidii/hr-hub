using BCrypt.Net;

// Run this code in your backend Program.cs or a test controller to generate correct hashes

public class CredentialGenerator
{
    public static void GenerateDemoCredentials()
    {
        Console.WriteLine("Generating Demo Credentials with BCrypt (Work Factor 12)");
        Console.WriteLine("=".PadRight(60, '='));

        string adminPassword = "admin123";
        string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 12);
        Console.WriteLine($"\nAdmin Credentials:");
        Console.WriteLine($"  Email: admin@sfon.com");
        Console.WriteLine($"  Password: {adminPassword}");
        Console.WriteLine($"  Bcrypt Hash: {adminHash}");

        string empPassword = "emp123";
        string empHash = BCrypt.Net.BCrypt.HashPassword(empPassword, workFactor: 12);
        Console.WriteLine($"\nEmployee Credentials:");
        Console.WriteLine($"  Email: emp@sfon.com");
        Console.WriteLine($"  Password: {empPassword}");
        Console.WriteLine($"  Bcrypt Hash: {empHash}");

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("SQL INSERT Statement:");
        Console.WriteLine("=".PadRight(60, '='));

        Console.WriteLine($@"
DELETE FROM tblUsers WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');

INSERT INTO tblUsers (FirstName, LastName, EmailId, Password, IsActive, CreatedAt, UpdatedAt, UserImage, EmployeeId)
VALUES 
    ('Admin', 'User', 'admin@sfon.com', '{adminHash}', 1, GETDATE(), GETDATE(), NULL, NULL),
    ('Employee', 'Test', 'emp@sfon.com', '{empHash}', 1, GETDATE(), GETDATE(), NULL, NULL);
");

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("Verification:");
        Console.WriteLine("=".PadRight(60, '='));

        // Verify passwords
        bool isAdminValid = BCrypt.Net.BCrypt.Verify(adminPassword, adminHash);
        bool isEmpValid = BCrypt.Net.BCrypt.Verify(empPassword, empHash);

        Console.WriteLine($"\nAdmin password verification: {(isAdminValid ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine($"Employee password verification: {(isEmpValid ? "✓ PASSED" : "✗ FAILED")}");
    }
}

// Usage: Call GenerateDemoCredentials() in your Program.cs or Startup
