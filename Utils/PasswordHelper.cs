// Utils/PasswordHelper.cs
using System;
using System.Security.Cryptography;
using System.Text;

namespace MedicalStoreMS.Utils
{
    public static class PasswordHelper
    {
        // Simple SHA256 hash — no salt, so passwords are straightforward
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;
    }
}