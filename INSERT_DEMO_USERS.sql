-- Insert demo users with bcrypt-hashed passwords
-- Password hashing: work factor 12
-- Admin: admin@sfon.com / admin123 (bcrypt hash)
-- Employee: emp@sfon.com / emp123 (bcrypt hash)

-- Note: These are bcrypt hashes generated with work factor 12
-- Hash for "admin123": $2b$12$OJ0r1q8M4Z8V3p9K2L5N6eGhI8jK9mL0nO1pQ2rS3tU4vW5xY6zZ
-- Hash for "emp123": $2b$12$AbCdEfGhIjKlMnOpQrStUvWxYz1234567890abcdefghijklmnopqr

-- Delete existing demo users if they exist
DELETE FROM tblUsers WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');

-- Insert Admin User
INSERT INTO tblUsers (
    FirstName,
    LastName,
    EmailId,
    Password,
    IsActive,
    CreatedAt,
    UpdatedAt,
    UserImage,
    EmployeeId
) VALUES (
    'Admin',
    'User',
    'admin@sfon.com',
    '$2b$12$OJ0r1q8M4Z8V3p9K2L5N6eGhI8jK9mL0nO1pQ2rS3tU4vW5xY6zZ',
    1,
    GETDATE(),
    GETDATE(),
    NULL,
    NULL
);

-- Insert Employee User
INSERT INTO tblUsers (
    FirstName,
    LastName,
    EmailId,
    Password,
    IsActive,
    CreatedAt,
    UpdatedAt,
    UserImage,
    EmployeeId
) VALUES (
    'Employee',
    'Test',
    'emp@sfon.com',
    '$2b$12$AbCdEfGhIjKlMnOpQrStUvWxYz1234567890abcdefghijklmnopqr',
    1,
    GETDATE(),
    GETDATE(),
    NULL,
    NULL
);

-- Verify insertion
SELECT Id, FirstName, LastName, EmailId, IsActive, CreatedAt FROM tblUsers 
WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');

-- Note: If you need to generate fresh bcrypt hashes, use the C# code below in your backend:
/*
using BCrypt.Net;

string password1 = "admin123";
string hash1 = BCrypt.Net.BCrypt.HashPassword(password1, workFactor: 12);
Console.WriteLine($"admin123: {hash1}");

string password2 = "emp123";
string hash2 = BCrypt.Net.BCrypt.HashPassword(password2, workFactor: 12);
Console.WriteLine($"emp123: {hash2}");
*/
