public class EncryptionSettings
{
    // Clave base64 de 32 bytes (AES-256). Debe vivir fuera del repositorio en producción
    // (variable de entorno), igual que Jwt:Key.
    public string Key { get; set; } = string.Empty;
}
