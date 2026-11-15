/// <summary>
/// Miniaturas de las fotos de guía: versión pequeña (~320 px) para listados, de modo que las pantallas no
/// descarguen la foto completa solo para mostrar un cuadrito. La foto completa se baja al abrirla.
/// </summary>
public static class ItemPhotoThumbnails
{
    public const int MaxSide = 320;
    public const int Quality = 70;

    /// <summary>
    /// Genera la miniatura a partir de la foto original y la guarda en el almacenamiento.
    /// Devuelve null si la imagen no se pudo procesar (la foto original sigue siendo válida).
    /// </summary>
    public static async Task<(string Path, byte[] Content)?> CreateAndStoreAsync(
        ItemPhoto photo,
        byte[] original,
        IImageCompressor compressor,
        IFileStorageService storage,
        CancellationToken cancellationToken = default)
    {
        var thumbnail = compressor.CreateThumbnail(original, MaxSide, Quality);
        if (thumbnail is null)
        {
            return null;
        }

        var baseName = System.IO.Path.GetFileNameWithoutExtension(photo.OriginalFileName ?? photo.FileName);
        var fileName = $"{(string.IsNullOrWhiteSpace(baseName) ? "foto" : baseName)}_thumb.jpg";

        using var stream = new MemoryStream(thumbnail);
        var upload = await storage.UploadAsync("Inspections", "Thumbnails", photo.WorkGuideMainId, fileName, "image/jpeg", stream, cancellationToken);
        return (upload.FilePath, thumbnail);
    }
}
