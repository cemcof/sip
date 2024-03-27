using System.ComponentModel.DataAnnotations.Schema;

namespace sip.Documents;

public class FileData
{
    public Guid Id { get; set; }

    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    [NotMapped] public int Length => Data.Length;
    

    public StreamReader AsTextStream() => new(new MemoryStream(Data), Encoding.UTF8);
    public MemoryStream AsByteStream() => new(Data, writable: false);

}