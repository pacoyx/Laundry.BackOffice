public class EmailAccount
{
    public int Id { get; set; }

    public string Operacion { get; set; } = string.Empty;

    public string Provider { get; set; } = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    // Guardado cifrado (AesSymmetricEncryptionService), nunca en texto plano.
    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
