// ReSharper disable InconsistentNaming

namespace sip.Experiments;

public class DataLifecycleOptions : IEquatable<DataLifecycleOptions>
{
    #region EQUALS_BOILERPLATE

    public bool Equals(DataLifecycleOptions? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DataLifecycleOptions) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(DataLifecycleOptions? left, DataLifecycleOptions? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DataLifecycleOptions? left, DataLifecycleOptions? right)
    {
        return !Equals(left, right);
    }

    #endregion

    [Required] public string Id { get; set; } = null!;
    [Required] public string DisplayName { get; set; } = null!;
    public string Tip { get; set; } = string.Empty;
    public IConfigurationSection Setup { get; set; } = null!;
}