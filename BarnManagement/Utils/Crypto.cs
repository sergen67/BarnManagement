using System;
using System.Security.Cryptography;
using System.Text;

namespace BarnManagement.WinForms.Utils
{
    public static class Crypto
    {
        public static byte[] NewSalt(int size = 16)
        {
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                return salt;
            }
        }



        public static byte[] HashPassword(string password, byte[] salt)
        {
            using (var sha = SHA256.Create())
            {
                var plain = Encoding.UTF8.GetBytes(password);
                var combo = new byte[salt.Length + plain.Length];
                Buffer.BlockCopy(salt, 0, combo, 0, salt.Length);
                Buffer.BlockCopy(plain, 0, combo, salt.Length, plain.Length);
                return sha.ComputeHash(combo);
            }
        }
    }
}
