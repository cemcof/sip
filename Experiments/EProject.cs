namespace sip.Experiments;

public class EProject : Project, IStringFilter
{
    public bool IsFilterMatch(string filter = "")
    {
        return StringUtils.IsFilterMatchAtLeastOneOf(filter, Title);
    }
}
