namespace sip.Experiments;

public class EProject : Project, IStringFilter
{
    public bool IsFilterMatch(string? filter = null)
    {
        return StringUtils.IsFilterMatchAtLeastOneOf(filter, Title);
    }
}
