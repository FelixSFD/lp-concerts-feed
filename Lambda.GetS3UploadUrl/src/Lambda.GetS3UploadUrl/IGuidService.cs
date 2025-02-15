namespace Lambda.GetS3UploadUrl;


/// <summary>
/// Wrapper to be able to inject random Guids
/// </summary>
public interface IGuidService
{
    /// <summary>
    /// Returns a new random GUID
    /// </summary>
    /// <returns>random GUID</returns>
    public Guid Random();
}