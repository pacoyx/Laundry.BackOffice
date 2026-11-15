using SkiaSharp;

public class SkiaImageCompressor : IImageCompressor
{
    public byte[] CompressToJpeg(byte[] originalImage, int maxWidth = 1000, int quality = 75)
    {
        using var original = SKBitmap.Decode(originalImage);
        if (original is null)
        {
            return originalImage;
        }

        var bitmapToEncode = original;
        SKBitmap? resized = null;

        if (original.Width > maxWidth)
        {
            var newHeight = (int)(original.Height * (maxWidth / (float)original.Width));
            resized = original.Resize(new SKImageInfo(maxWidth, newHeight), SKFilterQuality.Medium);
            if (resized is not null)
            {
                bitmapToEncode = resized;
            }
        }

        using var image = SKImage.FromBitmap(bitmapToEncode);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

        resized?.Dispose();

        return data.ToArray();
    }

    public byte[]? CreateThumbnail(byte[] originalImage, int maxSide = 320, int quality = 70)
    {
        try
        {
            using var skData = SKData.CreateCopy(originalImage);
            using var codec = SKCodec.Create(skData);
            if (codec is null)
            {
                return null;
            }

            var width = codec.Info.Width;
            var height = codec.Info.Height;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            var scale = Math.Min(1f, maxSide / (float)Math.Max(width, height));
            var targetWidth = Math.Max(1, (int)Math.Round(width * scale));
            var targetHeight = Math.Max(1, (int)Math.Round(height * scale));

            // JPEG permite decodificar directamente a 1/2, 1/4 u 1/8 del tamaño: se pide el doble del destino
            // para conservar nitidez y se evita reservar en memoria la foto completa (12 MP ≈ 48 MB).
            var decodeSize = codec.GetScaledDimensions(Math.Min(1f, scale * 2));
            var decodeInfo = new SKImageInfo(decodeSize.Width, decodeSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

            using var decoded = new SKBitmap(decodeInfo);
            var result = codec.GetPixels(decodeInfo, decoded.GetPixels());
            if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
            {
                return null;
            }

            using var resized = decoded.Resize(new SKImageInfo(targetWidth, targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul), SKFilterQuality.Medium);
            if (resized is null)
            {
                return null;
            }

            using var oriented = ApplyOrientation(resized, codec.EncodedOrigin);
            using var image = SKImage.FromBitmap(oriented);
            using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            return encoded?.ToArray();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }

    /// <summary>
    /// Devuelve una copia de la imagen con la orientación EXIF aplicada, sobre fondo blanco
    /// (para que imágenes con transparencia no queden negras al pasar a JPEG).
    /// </summary>
    private static SKBitmap ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        var w = source.Width;
        var h = source.Height;
        var swapsAxes = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

        var result = new SKBitmap(swapsAxes ? h : w, swapsAxes ? w : h, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.White);

        switch (origin)
        {
            case SKEncodedOrigin.TopRight:      // 2: espejo horizontal
                canvas.Translate(w, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:   // 3: 180°
                canvas.Translate(w, h);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft:    // 4: espejo vertical
                canvas.Translate(0, h);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:       // 5: transpuesta
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:      // 6: 90° horario (foto vertical típica)
                canvas.Translate(h, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:   // 7: transversa
                canvas.Translate(h, w);
                canvas.RotateDegrees(-90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftBottom:    // 8: 90° antihorario
                canvas.Translate(0, w);
                canvas.RotateDegrees(-90);
                break;
        }

        canvas.DrawBitmap(source, 0, 0);
        canvas.Flush();
        return result;
    }
}
