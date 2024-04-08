using System.Security.Cryptography;
using System.Text;

namespace NetBackend.Tools;

public class ComputeHash
{
    public static string ComputeSha256Hash(string rawData)
    {
        if (string.IsNullOrEmpty(rawData))
        {
            throw new ArgumentException("Raw data cannot be null or empty.", nameof(rawData));
        }
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        StringBuilder builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}