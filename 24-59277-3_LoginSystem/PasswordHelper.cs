using System;
using System.Security.Cryptography;
using System.Text;

namespace _24_59277_3_LoginSystem
{
    /// <summary>
    /// Turns a plain-text password into a SHA-256 hash and back-checks a
    /// candidate password against a stored hash. Only the hash is ever
    /// written to the database - the real password never touches SQL.
    /// SHA-256 is the minimum this lab asks for; a real production system
    /// would use a salted, slow hash like PBKDF2/BCrypt instead, since plain
    /// SHA-256 is fast and therefore brute-forceable at scale.
    /// </summary>
    internal static class PasswordHelper
    {
        public static string Hash(string plainTextPassword)
        {
            if (plainTextPassword == null) throw new ArgumentNullException(nameof(plainTextPassword));

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainTextPassword);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool Verify(string plainTextPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            string candidateHash = Hash(plainTextPassword);
            return string.Equals(candidateHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
