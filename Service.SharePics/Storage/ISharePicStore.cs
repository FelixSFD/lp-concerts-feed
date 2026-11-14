namespace Service.SharePics.Storage;

public interface ISharePicStore
{
    public Task StorePermanentAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken = default);
}
