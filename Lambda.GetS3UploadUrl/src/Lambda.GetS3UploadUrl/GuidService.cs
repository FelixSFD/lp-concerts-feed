namespace Lambda.GetS3UploadUrl;


/// <inheritdoc/>
public class GuidService : IGuidService
{
    /// <inheritdoc/>
    public Guid Random() => Guid.NewGuid();
}