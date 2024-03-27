// ReSharper disable NonReadonlyMemberInGetHashCode
namespace sip.Autoloaders;

public record AutoloaderInstrumentGroups(
    string Instrument,
    List<AutoloaderGroupInfo> AutoloaderGroupInfos
);

public record AutoloaderGroupInfo(
    string SampleDescriptions,
    string SampleLoadings,
    DateTime TimeFirst,
    DateTime TimeLast
)
{
    public TimeSpan TimeSpan => TimeLast - TimeFirst;
}

public record AutoloaderStates(
    Dictionary<DateTime, Dictionary<string, string>> States
)
{
    public DateTime FindLastTWithNames(List<string> positions)
    {
        return States
            .Reverse()
            .FirstOrDefault(s =>
                positions.Any(p => !string.IsNullOrEmpty(s.Value[p]))
            ).Key;
    }

    public AutoloaderStates FindPreviousStates(DateTime from)
    {
        var sts = States
            .Where(s => s.Key < from)
            .ToDictionary(k => k.Key, v => v.Value);

        return new AutoloaderStates(sts);
    }

    public IEnumerable<string> PickAllItems(string itemName)
    {
        return States
            .Where(s => s.Value.ContainsKey(itemName))
            .Select(s => s.Value[itemName]);
    }
    
    public DateTime TimeFirst => States.Keys.Min();
    public DateTime TimeLast => States.Keys.Max();

    public string PickAt(DateTime dt, string itemName)
    {
        if (States.ContainsKey(dt))
        {
            return States[dt].GetValueOrDefault(itemName, string.Empty);
        }

        return string.Empty;
    }

    public List<Dictionary<string, object>> SimplifyForGridUsageStats()
    {
        int GetRelevantGrid(Dictionary<string, string> simElem)
        {
            bool isStgValid = int.TryParse(simElem["Cartridge on CompuStage"], out int stg);
            bool isXferValid = int.TryParse(simElem["Cartridge in transfer"], out int xfer);
            if (!isStgValid) stg = 0;
            if (!isXferValid) xfer = 0;

            if (stg > 0 && xfer > 0)
            {
                // This is a super special case, indicating an error or ambiguous state
                return -(100 * stg + xfer);
            }

            if (stg > 0 || xfer > 0)
            {
                return Math.Max(stg, xfer);
            }

            return 0;
        }

        var result = new List<Dictionary<string, object>>();

        foreach (var state in States)
        {
            var d = new Dictionary<string, object>();
            d["_dt"] = state.Key;
            d["_grid"] = GetRelevantGrid(state.Value);

            foreach (var name in new List<string> { "Cartridge on CompuStage", "Cartridge in transfer", "LoadCartridgeCycles", "Action" })
            {
                d[name] = state.Value.GetValueOrDefault(name, "???");
            }

            result.Add(d);
        }

        return result;
    }
}

public record AutoloaderInstrumentData(
    string Instrument,
    // string SetDataFileName,
    List<AutoloaderStates> StatesContainers
);



