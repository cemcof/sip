// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace sip.Environment;

// TODO - is it ok to use readonly on model?

public class EnvironmentModel
{
    public DateTime DtDataGathered { get; set; }
    public List<SensorData> Sensors { get; set; } = new();
}

public class SensorData
{
    public string Sensor { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Dictionary<string, VariableData> Variables = new();
}

public class VariableData
{
    public string VarId { get; set; } = string.Empty;
    public string VarName { get; set; } = string.Empty;
    public double Min { get; set; }
    public double Max { get; set; }
    public double Current { get; set; }
    public byte[] PngGraph { get; set; } = Array.Empty<byte>();
}