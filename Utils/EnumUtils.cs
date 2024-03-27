using sip.Forms;

namespace sip.Utils;

public class EnumUtils
{
    public static Dictionary<string, string> EnumToOptions(Type enumType)
    {
        var result = new Dictionary<string, string>();
        var memberInfos = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var memberInfo in memberInfos)
        {
            var key = memberInfo.Name;
            var value = key.TitleCaseToText();
            var renderAttr = memberInfo.GetCustomAttribute<RenderAttribute>();
            if (renderAttr is not null && !string.IsNullOrEmpty(renderAttr.Title))
            {
                value = renderAttr.Title;
            }

            result[key] = value;
        }

        return result;
    }

    public static string EnumItemTitle<TEnum>(TEnum item) where TEnum : Enum
    {
        var opts = EnumToOptions(typeof(TEnum));
        return opts[item.ToString()];
    }
}