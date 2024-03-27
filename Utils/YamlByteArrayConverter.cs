namespace sip.Utils;

public class ByteArrayConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        // Return true to indicate that this converter is able to handle the byte[] type
        return type == typeof(byte[]);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current!;
        var bytes = Convert.FromBase64String(scalar.Value);
        parser.MoveNext();
        return bytes;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var bytes = (byte[])value!;
        emitter.Emit(new YamlDotNet.Core.Events.Scalar(
            null,
            "tag:yaml.org,2002:binary",
            Convert.ToBase64String(bytes),
            ScalarStyle.Plain,
            false,
            false
        ));
    }
}