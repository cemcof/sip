namespace sip.Autoloaders;

using States = Dictionary<DateTime, Dictionary<string, string>>;

public class AutoloadersService(IOptionsMonitor<AutoloadersOptions> options)
{
    private readonly Dictionary<string, List<AutoloaderInstrumentData>> _data    = new();  
    public           DateTime                                           DtLastUpdate { get; private set; }

    public void UpdateData(List<AutoloaderInstrumentData> data, IOrganization organization)
    {
        _data[organization.Id] = data;
        DtLastUpdate = DateTime.UtcNow;
    }

    public bool IsDataAvailable(IOrganization forOrganization)
        => _data.ContainsKey(forOrganization.Id);

    public List<AutoloaderInstrumentGroups> GetAutoloadersOverviews(IOrganization forOrganization)
    {
        // TODO - magic
        var result = new List<AutoloaderInstrumentGroups>();
        
        foreach (var autInstData in _data[forOrganization.Id])
        {
            var agroups = autInstData.StatesContainers.Select(s => new AutoloaderGroupInfo(
                SampleDescriptions: GenerateNiceDescriptions(s, options.Get(forOrganization)),
                SampleLoadings: GenerateNiceLoadings(s, options.Get(forOrganization)),
                TimeFirst: s.TimeFirst,
                TimeLast: s.TimeLast
                ));
            
            result.Add(new AutoloaderInstrumentGroups(autInstData.Instrument, agroups.ToList()));
        }

        return result;
    }

    public string GenerateNiceDescriptions(AutoloaderStates states, AutoloadersOptions opts)
    {
        var slotNames = opts.Positions.Select(p => $"Slot {SlotNumFormat(p)} description").ToList(); 
        var result = new StringBuilder();
        result.Append("POSITION     RECENT NAME                    (PREVIOUS NAMES)\n");

        var previousDescriptionStates = new AutoloaderStates(new States());
        var lastDtWithNames = states.FindLastTWithNames(slotNames);
        if (lastDtWithNames != default)
        {
            previousDescriptionStates = states.FindPreviousStates(lastDtWithNames);
        }

        var atLeastOnePositionUsed = false;
        foreach (var i in opts.Positions.AsEnumerable().Reverse().ToList())
        {
            // Determine if this position was used 
            var positionUsed = states.PickAllItems($"Cartridge in Cassette slot {SlotNumFormat(i)}")
                .Any(s => s != "-2" && s != "0");
            var positionUsedCaption = (positionUsed) ? "(LOADED)" : "(empty)";
            if (positionUsed) atLeastOnePositionUsed = true;
            
            // Determine descriptions of the position
            var lastDescription = states.PickAt(lastDtWithNames, $"Slot {SlotNumFormat(i)} description");
            var previousDescriptions = previousDescriptionStates
                .PickAllItems($"Slot {SlotNumFormat(i)} description")
                .Where(d => !string.IsNullOrWhiteSpace(d) && d != lastDescription)
                .Distinct()
                .ToList();
            var descSeparator = FindBestSeparator(previousDescriptions);
            var previousDescriptionsJoined = "(" + string.Join(descSeparator, previousDescriptions) + ")";
            
            // Add final row
            result.Append($"{positionUsedCaption,-8} {i:D2}: {lastDescription,-30} {previousDescriptionsJoined}\n");
        }

        if (!atLeastOnePositionUsed)
            result.Append("WARNING: no position used ?!?");

        return result.ToString();
    }

    public string SlotNumFormat(int slot)
        => slot >= 0 ? $"{slot:D2}" : $"{slot:D1}";

    public string GenerateNiceLoadings(AutoloaderStates states, AutoloadersOptions opts)
    {
        // Generated by ChatGPT from python implementation
        
        var sim = states.SimplifyForGridUsageStats();
        
        var res = new List<string>
        {
            "TIME    AL POSITION    APPROX. DURATION"
        };

        var stage0 = int.Parse(states.PickAt(states.TimeFirst, "Cartridge on CompuStage"));
        if (stage0 != 0)
        {
            res.Add("(there might have been sample on stage before docking)");
        }
        
        var prevDt = (DateTime)sim[0]["_dt"];
        var prevPos = (int)sim[0]["_grid"];
        foreach (var element in sim)
        {
            var pos = (int)element["_grid"];
            if (pos != prevPos || (string)element["Action"] == "Mapping Cassette Slot")
            {
                var strPos = prevPos.ToString().PadLeft(9);
                if ((string)element["Action"] == "Mapping Cassette Slot")
                {
                    strPos = "inventory".PadLeft(9);
                }
                var dt = (DateTime)element["_dt"];
                var timeStringStart = prevDt.ToString("HH:mm");
                var delta = dt - prevDt;
                var deltaHours = delta.TotalHours;
                var flag = "";
                if (deltaHours > opts.MinimalTimeToReport.TotalHours && prevPos != 0)
                {
                    flag = " <<<";
                }
                res.Add($"{timeStringStart}    {strPos}       {deltaHours:F2} h{flag}");
                prevPos = pos;
                prevDt = dt;
            }
        }

        // Final entry
        var finalElem = sim.Last();
        var finalTimeStringStart = prevDt.ToString("HH:mm");
        var finalStrPos = $"{prevPos,9}";
        if ((string)finalElem["Action"] == "Mapping Cassette Slot")
        {
            finalStrPos = "inventory".PadLeft(9);
        }
        var finalDelta = ((DateTime)sim[^1]["_dt"]) - prevDt;
        var finalDeltaHours = finalDelta.TotalHours;
        var finalFlag = "";
        if (finalDeltaHours > opts.MinimalTimeToReport.TotalHours && prevPos != 0)
        {
            finalFlag = " <<<";
        }
        res.Add($"{finalTimeStringStart}    {finalStrPos}       {finalDeltaHours:F2} h{finalFlag}");

        var stage1 = int.Parse(states.PickAt(states.TimeLast, "Cartridge on CompuStage"));
        if (stage1 != 0)
        {
            res.Add("(there might have been sample on stage after undocking)");
        }

        var lcc0 = Convert.ToInt32(sim[0]["LoadCartridgeCycles"]);
        var lcc1 = Convert.ToInt32(sim[^1]["LoadCartridgeCycles"]);
        res.Add($"There were {lcc1 - lcc0} grid operations (load / unload / exchange)");

        return string.Join("\n", res);
    }
    
    public string FindBestSeparator(List<string> lst)
    {
        var commaFound = false;
        var semicolonFound = false;
        foreach (var s in lst)
        {
            if (s.Contains(','))
            {
                commaFound = true;
            }
            if (s.Contains(';'))
            {
                semicolonFound = true;
            }
        }

        var sep = ", ";
        if (commaFound)
        {
            sep = "; ";
        }
        if (semicolonFound)
        {
            sep = " | ";
        }

        return sep;
    }


}