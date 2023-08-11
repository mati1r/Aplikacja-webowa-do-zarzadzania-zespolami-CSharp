using System.Security.Cryptography;
using System.Text;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Validation
{
    public static class Hash
    {
        public static string GenerateSalt(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder salt = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                salt.Append(chars[random.Next(chars.Length)]);
            }

            return salt.ToString();
        }

        public static string HashPassword(string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

                //Salt the hash
                byte[] combinedBytes = new byte[passwordBytes.Length + saltBytes.Length];
                Array.Copy(passwordBytes, combinedBytes, passwordBytes.Length);
                Array.Copy(saltBytes, 0, combinedBytes, passwordBytes.Length, saltBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(combinedBytes);

                StringBuilder builder = new StringBuilder();
                //Changing bytes to hexadecimal
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string inputPassword, string hashedPassword, string salt)
        {
            string inputHashed = HashPassword(inputPassword, salt);
            return string.Equals(inputHashed, hashedPassword);
        }

    }
}
