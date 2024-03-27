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

    public const string PROTOCOL_ID_KEY = "ID";
    public const string PROTOCOL_TYPE_KEY = "TYPE";
    public const string PROTOCOL_DESCRIPTION_KEY = "DESCRIPTION";
    public const string PROTOCOL_NAME_KEY = "NAME";

    public string Id { get; set; } = null!;
    public string Title { get; set; } = "<untitled workflow>";
    public string Description { get; set; } = "<no description>";

    public string? Diagram { get; set; }
    public string? Provider { get; set; }

    public List<string> Engines { get; set; } = new();


    public IConfigurationSection Protocols { get; set; } = null!;

}

