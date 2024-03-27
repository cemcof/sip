namespace sip.Projects.Statuses;

public class StatusInfo(string id) : IEquatable<StatusInfo>
{
    [StringLength(128)] public string Id { get; } = id;

    public string DisplayName { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;

    public ICollection<Status> StatusesOfStatusInfo { get; set; } = null!;

    #region EQUALS_BOILERPLATE
    public bool Equals(StatusInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StatusInfo) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(StatusInfo? left, StatusInfo? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StatusInfo? left, StatusInfo? right)
    {
        return !Equals(left, right);
    }
    #endregion
}