namespace sip.Experiments.Workflows;

public record WorkflowhubOptions
{
    [Required] public string BaseUrl { get; set; } = null!;
    
    /// <summary>
    /// Having CollectionId as null means WFH is disabled
    /// </summary>
    public string? CollectionId { get; set; }
    public string? Pattern { get; set; }
    public TimeSpan CacheTime { get; set; } = TimeSpan.FromDays(1);
}