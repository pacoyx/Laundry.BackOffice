using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private bool _containerEnsured;

    public AzureBlobStorageService(IOptions<FileStorageSettings> settings)
    {
        var blobServiceClient = new BlobServiceClient(settings.Value.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(settings.Value.ContainerName);
    }

    public async Task<StorageFileResult> UploadAsync(
        string topFolder,
        string category,
        long entityId,
        string originalFileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        await EnsureContainerAsync(cancellationToken);

        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{topFolder.ToLowerInvariant()}-{entityId}-{Guid.NewGuid():N}{extension}";
        var blobPath = $"{topFolder}/{entityId}/{category.ToLowerInvariant()}/{storedFileName}";

        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        return new StorageFileResult(storedFileName, blobPath, properties.Value.ContentLength);
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureContainerAsync(cancellationToken);

        var blobClient = _containerClient.GetBlobClient(filePath);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureContainerAsync(cancellationToken);

        var blobClient = _containerClient.GetBlobClient(filePath);

        // OpenReadAsync (en vez de DownloadStreamingAsync) devuelve un stream que SI soporta
        // Seek, descargando el contenido por partes a medida que se lee. Los endpoints que
        // sirven este stream usan Results.File(..., enableRangeProcessing: true), y ese soporte
        // de rangos de ASP.NET Core necesita poder hacer seek sobre el stream -con
        // DownloadStreamingAsync (stream de solo avance) las peticiones Range de algunos
        // navegadores moviles (ej. Safari/iOS) se quedaban colgadas en vez de fallar o servir
        // el archivo completo.
        return await blobClient.OpenReadAsync(position: 0, cancellationToken: cancellationToken);
    }

    private async Task EnsureContainerAsync(CancellationToken cancellationToken)
    {
        if (_containerEnsured)
        {
            return;
        }

        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        _containerEnsured = true;
    }
}
