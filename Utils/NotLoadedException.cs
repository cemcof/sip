namespace sip.Utils;

public class NotLoadedException(string message) : Exception(message)
{
    public static NotLoadedException ThrowForType<TType>()
    {
        return new NotLoadedException($"Object(s) not loaded, {typeof(TType).Name}");
    }
}