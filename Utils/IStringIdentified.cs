namespace sip.Utils;

public interface IStringIdentified
{
    public string Id => GetType().Name;
}