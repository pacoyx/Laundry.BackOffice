using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

// Cifrado reversible para datos que el sistema necesita volver a leer en texto plano
// (ej. password SMTP para autenticar contra el servidor de correo). No confundir con
// IEncryptService, que solo hashea contraseñas de login de forma irreversible.
public class AesSymmetricEncryptionService : ISymmetricEncryptionService
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    private readonly byte[] _key;

    public AesSymmetricEncryptionService(IOptions<EncryptionSettings> settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.Key))
        {
            throw new InvalidOperationException("Encryption:Key no está configurada.");
        }

        _key = Convert.FromBase64String(settings.Value.Key);
    }

    public string Encrypt(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aesGcm = new AesGcm(_key, TagSizeBytes);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var data = Convert.FromBase64String(cipherText);

        var nonce = data[..NonceSizeBytes];
        var tag = data[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
        var cipherBytes = data[(NonceSizeBytes + TagSizeBytes)..];
        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(_key, TagSizeBytes);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
