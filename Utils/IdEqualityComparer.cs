namespace sip.Utils;

public interface IIdentified<out TId>
{
    TId Id { get; }
}

public class IdEqualityComparer<T, TId> : EqualityComparer<T> where T : IIdentified<TId>
{
    public override bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null ||
            Default.Equals(x, default) ||
            y is null ||
            Default.Equals(y, default))
            return false;

        // From previous equals on default, it is certain that none of the ids are null
        return x.Id!.Equals(y.Id);
    }

    public override int GetHashCode(T obj) =>
        obj.Id?.GetHashCode() ?? 0;

    public static readonly IEqualityComparer<T> Comparer = new IdEqualityComparer<T, TId>();
}    