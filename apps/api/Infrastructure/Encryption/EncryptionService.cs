using System.Security.Cryptography;

namespace BrowserAgent.Api.Infrastructure.Encryption;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string base64Key)
    {
        var keyBytes = Convert.FromBase64String(base64Key);
        if (keyBytes.Length != 32)
        {
            throw new ArgumentException("Encryption key must be 32 bytes (256 bits).", nameof(base64Key));
        }
        _key = keyBytes;
    }

    public string Encrypt(string plainText)
    {
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var cipher = new byte[plainBytes.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var result = new byte[12 + 16 + cipher.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, 12);
        Buffer.BlockCopy(tag, 0, result, 12, 16);
        Buffer.BlockCopy(cipher, 0, result, 28, cipher.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var data = Convert.FromBase64String(cipherText);

        if (data.Length < 28)
        {
            throw new CryptographicException("Invalid cipher text.");
        }

        var nonce = new byte[12];
        var tag = new byte[16];
        var cipher = new byte[data.Length - 28];

        Buffer.BlockCopy(data, 0, nonce, 0, 12);
        Buffer.BlockCopy(data, 12, tag, 0, 16);
        Buffer.BlockCopy(data, 28, cipher, 0, cipher.Length);

        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, cipher, tag, plain);

        return System.Text.Encoding.UTF8.GetString(plain);
    }
}
