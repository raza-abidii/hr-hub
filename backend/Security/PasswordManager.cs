using BCrypt.Net;

namespace EMSSolution.Security
{
    /// <summary>
    /// Utility class for password hashing and verification using bcrypt
    /// </summary>
    public static class PasswordManager
    {
        /// <summary>
        /// Hash a plain text password using bcrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="workFactor">BCrypt work factor (default: 12, recommended: 10-12)</param>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string password, int workFactor = 12)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: workFactor);
        }

        /// <summary>
        /// Verify a plain text password against a bcrypt hash
        /// </summary>
        /// <param name="inputPassword">Plain text password to verify</param>
        /// <param name="storedHash">Stored bcrypt hash or plain text password</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                // Check if the stored password is a bcrypt hash
                if (IsBcryptHash(storedHash))
                {
                    return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
                }
                
                // Fallback to plain text comparison for legacy passwords
                return inputPassword == storedHash;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password verification error: {ex.Message}");
                // If bcrypt verification fails, try plain text comparison
                return inputPassword == storedHash;
            }
        }

        /// <summary>
        /// Check if a string is a bcrypt hash
        /// </summary>
        /// <param name="hash">String to check</param>
        /// <returns>True if it appears to be a bcrypt hash</returns>
        public static bool IsBcryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            // Bcrypt hashes start with $2a$, $2b$, or $2y$ followed by work factor
            return hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$");
        }

        /// <summary>
        /// Generate a random secure password
        /// </summary>
        /// <param name="length">Password length (minimum: 8, default: 12)</param>
        /// <returns>Random password</returns>
        public static string GenerateSecurePassword(int length = 12)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            var random = new Random();
            var password = new System.Text.StringBuilder();

            for (int i = 0; i < length; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            return password.ToString();
        }
    }
}
