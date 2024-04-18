using sip.Forms.Dynamic;

namespace sip.Experiments.Workflows;

public class Workflow : IEquatable<Workflow>
{
    #region EQUALS
        public bool Equals(Workflow? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Title == other.Title && Provider == other.Provider;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Workflow) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, Provider);
        }

        public static bool operator ==(Workflow? left, Workflow? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Workflow? left, Workflow? right)
        {
            return !Equals(left, right);
        }
    #endregion

    

    public string Id { get; set; } = null!;
    public string Title { get; set; } = "<untitled workflow>";
    public string Description { get; set; } = "<no description>";

    public string? Diagram { get; set; }
    public string? Provider { get; set; }

    public List<string> Tags { get; set; } = new();

    public object? Data { get; set; } = new Dictionary<string, object>();
    
    // Only for automatic configuration binding
    public IConfigurationSection DataConf
    {
        // TODO - maybe we must have getter otherwise binder will ignore this
        // get
        // {
        //     if (Data is null) return new ConfigurationRoot(ArraySegment<IConfigurationProvider>.Empty);
        //     throw new NotSupportedException();
        // };
        set => Data = value.ToObject();
    }

}

