using System.Text;

namespace Service.SharePics.Gotenberg;

public class GotenbergResourceFile
{
    /// <summary>
    /// Name of the file as it is used in the template
    /// </summary>
    public required string FileName { get; set; }
    
    /// <summary>
    /// Content of the file as a byte array
    /// </summary>
    public required byte[] FileContent { get; set; }

    /// <summary>
    /// Optional encoding of the file content. If not specified, binary is assumed.
    /// </summary>
    public Encoding? FileEncoding { get; set; }
}