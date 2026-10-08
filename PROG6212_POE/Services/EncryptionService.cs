using System.Security.Cryptography;

namespace PROG6212_POE.Services
{
    public class EncryptionService
    {
        // 256-bit key (32 bytes) and 128-bit IV (16 bytes)
        private static readonly byte[] key = Convert.FromHexString("4b4a531a06a5a06f6e772523369883cf4b0126acd3201447432b6e43d7b723aa");
        private static readonly byte[] iv = Convert.FromHexString("161979AD5E1ECF561DB41822E672E0BF");

        // Method to perform encryption/decryption
        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        // Method to encrypt data
        public static byte[] EncryptData(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        // Method to decrypt data
        public static byte[] DecryptData(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }
    }
}
