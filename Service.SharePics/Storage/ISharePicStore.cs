namespace Service.SharePics.Storage;

/// <summary>
/// Stores files
/// </summary>
public interface ISharePicStore
{
    /// <summary>
    /// Stores a file permanently
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileKey">Filename or object key</param>
    /// <param name="cancellationToken">Token to cancel the upload</param>
    /// <returns></returns>
    public Task StorePermanentAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a file temporarily. (This might not have an effect in every file system)
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileKey">Filename or object key</param>
    /// <param name="cancellationToken">Token to cancel the upload</param>
    /// <returns></returns>
    public Task StoreTemporaryAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken = default);
}
