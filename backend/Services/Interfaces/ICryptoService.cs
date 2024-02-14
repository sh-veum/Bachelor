namespace NetBackend.Services.Interfaces;

public interface ICryptoService
{
    string Encrypt(string text, string secretKey);
    string Decrypt(string cipherText, string secretKey);
}