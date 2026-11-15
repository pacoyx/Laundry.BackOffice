public interface IImageCompressor
{
    byte[] CompressToJpeg(byte[] originalImage, int maxWidth = 1000, int quality = 75);

    /// <summary>
    /// Genera una miniatura JPEG cuyo lado mayor no supera <paramref name="maxSide"/>, aplicando la
    /// orientación EXIF (fotos de cámara). Devuelve null si la imagen no se puede decodificar.
    /// </summary>
    byte[]? CreateThumbnail(byte[] originalImage, int maxSide = 320, int quality = 70);
}
