namespace sip.Experiments.Workflows;

public record WorfklowHubOptions
{
    [Required] public string BaseUrl { get; set; } = null!;
    [Required] public string CollectionId { get; set; } = null!;
    public string? Pattern { get; set; }
    public TimeSpan CacheTime { get; set; } = TimeSpan.FromDays(1);
}