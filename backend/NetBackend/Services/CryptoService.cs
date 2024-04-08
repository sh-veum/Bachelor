using System.Security.Cryptography;
using System.Text;
using NetBackend.Services.Interfaces;

namespace Netbackend.Services;

public class CryptoService : ICryptoService
{
    public string Encrypt(string text, string secretKey)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = AdjustKeySize(secretKey, 32);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(text);
            }

            array = memoryStream.ToArray();
        }

        return Convert.ToBase64String(array);
    }

    public string Decrypt(string cipherText, string secretKey)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = AdjustKeySize(secretKey, 32);
        aes.IV = iv;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }

    private static byte[] AdjustKeySize(string secretKey, int size)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        Array.Resize(ref keyBytes, size); // Resize to ensure the key is of the correct size
        return keyBytes;
    }
}