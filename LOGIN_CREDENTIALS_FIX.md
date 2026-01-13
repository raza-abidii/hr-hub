# 🔐 Fix Login Credentials Issue

## 🐛 Problems Found & Fixed

### Problem 1: API Endpoint Mismatch ✅ FIXED
- **Issue**: Frontend was calling `/Account/Login` (old MVC endpoint)
- **Fix**: Updated to `/auth/login` (new API endpoint)
- **File**: `frontend/src/lib/api-client.ts`

### Problem 2: Password Field Name Mismatch ✅ FIXED
- **Issue**: Controller was looking for `PasswordHash` field, but model uses `Password`
- **Fix**: Changed to use `user.Password` directly
- **File**: `backend/Controllers/AuthApiController.cs`

### Problem 3: Demo Users Not in Database ⚠️ ACTION NEEDED
- **Issue**: `admin@sfon.com` and `emp@sfon.com` don't exist in database
- **Solution**: Run SQL script to insert them with correct bcrypt hashes
- **Files**: `INSERT_DEMO_USERS.sql`

---

## ✅ What Was Fixed in Code

### Frontend (api-client.ts)
```typescript
// ❌ Before
async login(email: string, password: string) {
  return this.client.post('/Account/Login', { email, password });
}

// ✅ After
async login(email: string, password: string) {
  return this.client.post('/auth/login', { email, password });
}
```

### Backend (AuthApiController.cs)
```csharp
// ❌ Before
if (!VerifyPassword(request.Password, user.PasswordHash ?? user.Password))

// ✅ After
if (!VerifyPassword(request.Password, user.Password))
```

---

## 🚀 How to Get Login Working

### Step 1: Generate Correct Bcrypt Hashes

Run this C# code to generate correct password hashes:

```csharp
using BCrypt.Net;

string adminPassword = "admin123";
string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 12);
// Output: $2b$12$XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

string empPassword = "emp123";
string empHash = BCrypt.Net.BCrypt.HashPassword(empPassword, workFactor: 12);
// Output: $2b$12$YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
```

**Option A: Run in C# Program**
```csharp
// Add this to your backend's Program.cs temporarily:
using BCrypt.Net;

var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12);
var empHash = BCrypt.Net.BCrypt.HashPassword("emp123", workFactor: 12);

Console.WriteLine($"Admin: {adminHash}");
Console.WriteLine($"Employee: {empHash}");

// Copy the hashes and delete this code
```

**Option B: Use the Provided File**
- A file `CredentialGenerator.cs` has been created in the backend folder
- You can reference it to generate hashes

### Step 2: Insert Users into Database

Once you have the correct hashes from Step 1:

1. Open SQL Server Management Studio
2. Connect to: `172.16.16.11` / Database: `EmpAttendanceTest`
3. Run this SQL (replace the hash values with YOUR generated hashes):

```sql
DELETE FROM tblUsers WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');

INSERT INTO tblUsers (FirstName, LastName, EmailId, Password, IsActive, CreatedAt, UpdatedAt, UserImage, EmployeeId)
VALUES 
    ('Admin', 'User', 'admin@sfon.com', '[PASTE_ADMIN_HASH_HERE]', 1, GETDATE(), GETDATE(), NULL, NULL),
    ('Employee', 'Test', 'emp@sfon.com', '[PASTE_EMP_HASH_HERE]', 1, GETDATE(), GETDATE(), NULL, NULL);

-- Verify
SELECT Id, FirstName, LastName, EmailId, IsActive FROM tblUsers 
WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');
```

### Step 3: Restart Backend

```powershell
cd backend

# Stop any running instance
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force

# Start fresh
dotnet run
```

### Step 4: Test Login

1. Open http://localhost:5175
2. Click "Login"
3. Enter credentials:
   - **Email**: `admin@sfon.com`
   - **Password**: `admin123`
4. Click "Sign In"

Expected: ✅ Successful login → Dashboard

---

## 📋 Pre-Generated Hashes (Do NOT use these - they're examples!)

These are EXAMPLE bcrypt hashes. Your actual hashes will be different (bcrypt generates new hashes each time):

```
Admin (admin123 - EXAMPLE):
$2b$12$OJ0r1q8M4Z8V3p9K2L5N6eGhI8jK9mL0nO1pQ2rS3tU4vW5xY6zZ

Employee (emp123 - EXAMPLE):
$2b$12$AbCdEfGhIjKlMnOpQrStUvWxYz1234567890abcdefghijklmnopqr
```

⚠️ **IMPORTANT**: These hashes are just examples. You MUST generate your own using the C# code above.

---

## 🔍 Troubleshooting

### "Invalid email or password" after all steps

**Check 1: Verify users exist in database**
```sql
SELECT * FROM tblUsers WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');
-- Should return 2 rows
```

**Check 2: Verify bcrypt hash format**
```sql
-- Each hash should start with $2b$12$
SELECT EmailId, Password 
FROM tblUsers 
WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');

-- Look for hashes like: $2b$12$XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

**Check 3: Verify IsActive is 1**
```sql
SELECT EmailId, IsActive FROM tblUsers 
WHERE EmailId IN ('admin@sfon.com', 'emp@sfon.com');
-- Should show IsActive = 1 for both
```

**Check 4: Test bcrypt verification in code**
```csharp
// Add a test endpoint temporarily in AuthApiController:
[HttpGet("test-verify/{password}")]
public IActionResult TestVerify(string password)
{
    var user = _db.Users.FirstOrDefault(u => u.EmailId == "admin@sfon.com");
    if (user == null) return BadRequest("User not found");
    
    bool isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
    return Ok(new { 
        emailId = user.EmailId,
        inputPassword = password,
        storedHash = user.Password,
        isValid = isValid
    });
}

// Then test: GET /api/auth/test-verify/admin123
```

### Backend won't start

```powershell
cd backend

# Kill running processes
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean build
dotnet clean
dotnet build

# Run
dotnet run
```

### Frontend shows CORS error

Make sure backend is running:
```powershell
cd backend
dotnet run
# Should show: Now listening on: http://localhost:5000
```

Check `.env.local` in frontend:
```
VITE_API_URL=http://localhost:5000/api
```

---

## 📊 Complete Login Flow

```
1. User enters: admin@sfon.com / admin123
   ↓
2. Frontend sends: POST /api/auth/login
   ↓
3. Backend receives request
   ↓
4. Find user by EmailId = "admin@sfon.com" in database
   ↓
5. Verify: BCrypt.Verify("admin123", stored_hash) ← Your generated hash!
   ↓
6. Create session
   ↓
7. Return: { success: true, user: {...}, token: "..." }
   ↓
8. Frontend: Store token, redirect to dashboard
   ↓
9. ✅ Logged in!
```

---

## ✨ Summary of Changes

| Item | Before | After | Status |
|------|--------|-------|--------|
| Frontend API endpoint | `/Account/Login` | `/auth/login` | ✅ Fixed |
| Backend password field | `user.PasswordHash ?? user.Password` | `user.Password` | ✅ Fixed |
| Demo users in DB | Not present | Need to insert with bcrypt | ⏳ Action needed |
| Bcrypt hashing | Not used | Using work factor 12 | ✅ Ready |

---

## 🎯 Next Steps

1. ✅ Code fixes applied (API endpoint, password field)
2. ⏳ **Generate bcrypt hashes** using the C# code
3. ⏳ **Insert demo users** using SQL script
4. ⏳ **Restart backend** (`dotnet run`)
5. ⏳ **Test login** (admin@sfon.com / admin123)

**Time to complete**: ~5 minutes

---

**Updated**: January 13, 2026  
**Status**: Code fixes applied ✅ | Demo users pending ⏳  
**Next**: Insert demo credentials and test login
