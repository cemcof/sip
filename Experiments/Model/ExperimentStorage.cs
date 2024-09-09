using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sip.Experiments.Model;

public class ExperimentStorage
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }
    [JsonIgnore, YamlIgnore] public Experiment Experiment { get; set; } = null!;
    
    public string StorageEngine { get; set; } = null!;

    public string? Target { get; set; }
    public string? Path { get; set; }
    public string? SubPath { get; set; }
    [NotMapped] public string? FullPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SubPath) || string.IsNullOrWhiteSpace(Path))
                return null;
            return System.IO.Path.Combine(Path, SubPath);
        }
    }

    public string? Token { get; set; }

    public bool Archive { get; set; } = false;

    public DateTime DtExpiration { get; set; }
    public TimeSpan ExpirationPeriod { get; set; }

    public StorageState State { get; set; }
    public DateTime DtLastUpdate { get; set; }
    [MaxLength(64)] public string? Node { get; set; }
}

public enum StorageState
{
    Uninitialized,
    Idle,
    TransferStartRequested,
    Transfering,
    TransferStopRequested, 
    ArchivationRequested,
    Archiving,
    Archived,
    ExpirationRequested,
    Expiring,
    Expired
}