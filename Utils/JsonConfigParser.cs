using System.Diagnostics;
using System.Text.Json;

namespace sip.Utils;

/// <summary>
/// Same as Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser
/// Sadly that class is internal, therefore we need a copy.
///
/// There are several adjustments
/// - throwing exceptions directly instead of using SR class.
/// - added Parse(string input) overload method for parsing strings conveniently.
/// - added Parse(JsonElement input) overload method for parsing JsonElement conveniently.
/// </summary>
public class JsonConfigParser
{
    private JsonConfigParser() { }

    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _paths = new();

    public static IDictionary<string, string?> Parse(Stream input)
        => new JsonConfigParser().ParseStream(input);

    public static IDictionary<string, string?> Parse(string input)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(input);
        writer.Flush();
        stream.Position = 0;
        return Parse(stream);
    }
    
    public static IDictionary<string, string?> Parse(JsonElement input)
        => new JsonConfigParser().ParseElement(input);

    private Dictionary<string, string?> ParseStream(Stream input)
    {
        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using (var reader = new StreamReader(input))
        using (JsonDocument doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("Configuration root JSON element must be an object, not " +
                                          doc.RootElement.ValueKind);
            }
            VisitObjectElement(doc.RootElement);
        }

        return _data;
    }

    private IDictionary<string, string?> ParseElement(JsonElement input)
    {
        if (input.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Configuration root JSON element must be an object, not " +
                                      input.ValueKind);
        }
        
        VisitObjectElement(input);
        
        return _data;
    }

    private IDictionary<string, string?> ParseArrayElement(string prefix, JsonElement input)
    {
        if (input.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("Configuration root JSON element must be an array, not " +
                                      input.ValueKind);
        }
        
        var index = 0;
        foreach (JsonElement element in input.EnumerateArray())
        {
            EnterContext(prefix + ":" + index);
            VisitValue(element);
            ExitContext();
            index++;
        }

        return _data;
    }

    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }

        SetNullIfElementIsEmpty(isEmpty);
    }

    private void VisitArrayElement(JsonElement element)
    {
        int index = 0;

        foreach (JsonElement arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString());
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _paths.Count > 0)
        {
            _data[_paths.Peek()] = null;
        }
    }

    private void VisitValue(JsonElement value)
    {
        Debug.Assert(_paths.Count > 0);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;

            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                string key = _paths.Peek();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException("Duplicate key " + key);
                }
                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException("Unsupported JSON token " + value.ValueKind);
        }
    }

    private void EnterContext(string context) =>
        _paths.Push(_paths.Count > 0 ?
            _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
            context);

    private void ExitContext() => _paths.Pop();

    public static IDictionary<string, string?> Parse(string workflow, JsonElement workflowRootElement)
        => new JsonConfigParser().ParseArrayElement(workflow, workflowRootElement);

    
}