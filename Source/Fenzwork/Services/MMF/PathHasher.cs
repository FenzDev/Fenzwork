using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Fenzwork.Services.MMF
{
    public static class PathHasher
    {
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string HashFilename(string filename, int maxLength)
        {
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(filename));
            string base62 = ToBase62(hashBytes);
            return base62.Substring(0, Math.Min(maxLength, base62.Length));
        }

        private static string ToBase62(byte[] bytes)
        {
            var value = new System.Numerics.BigInteger(bytes.Concat(new byte[] { 0 }).ToArray()); // avoid negative BigInteger
            var sb = new StringBuilder();

            while (value > 0)
            {
                value = System.Numerics.BigInteger.DivRem(value, 62, out var remainder);
                sb.Insert(0, Base62Chars[(int)remainder]);
            }

            return sb.ToString();
        }
    }

}
