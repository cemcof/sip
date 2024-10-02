using System.Globalization;
using Humanizer.Localisation;

namespace sip.Utils;

public static class DateUtils
{
    public static string StandardFormat(this DateTime dateTime, bool includeSec = false)
    {
        var dtLocal = dateTime.ToLocalTime();
        
        if (dateTime == dateTime.Date)
        {
            return dtLocal.ToString("dd.MM.yyyy");
        }
        
        var format = includeSec ? "dd.MM.yyyy HH:mm:ss" : "dd.MM.yyyy HH:mm"; 
            
        return dtLocal.ToString(format);
    }

    public static string StandardTsFormat(this TimeSpan timeSpan)
    {
        return timeSpan.ToString(@"d\.hh\:mm\:ss");
    }

    public static string HappenAgo(this DateTime when, bool stripSeconds = true)
    {
        // When working with time delta, we want to use UTC
        // We assume that unspecified timezone is UTC
        when = (when.Kind is DateTimeKind.Local) ? when.ToUniversalTime() : when;
        var compareAgainst = DateTime.UtcNow;
            
        if (stripSeconds)
        {
            when = when.StripSeconds();
            compareAgainst = compareAgainst.StripSeconds();
        }

        return when.Humanize(dateToCompareAgainst: DateTime.UtcNow, utcDate: true);
    }
    
    public static string HumanizeTimedelta(this DateTime when,
        DateTime? compareAgainst = null,
        bool countEmptyUnits = false,
        int precision = 2,
        bool toWords = false,
        string collectionSeparator = ", ",
        TimeUnit minUnit = TimeUnit.Second,
        TimeUnit maxUnit = TimeUnit.Year,
        CultureInfo? culture = null
        )
    {
        when = (when.Kind is DateTimeKind.Local) ? when.ToUniversalTime() : when;
        
        var against = compareAgainst ?? DateTime.UtcNow;
        against = (against.Kind is DateTimeKind.Local) ? against.ToUniversalTime() : against;
        
        var delta = when - against;
        var hum = delta.Humanize(precision, countEmptyUnits, culture, maxUnit, minUnit, collectionSeparator, toWords);
        // Add from now or ago
        return when < against ? $"{hum} ago" : $"in {hum}";
    }

    public static DateTime StripSeconds(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, kind: dt.Kind);
    }

    public static string ToPyCompatibleUtcIso(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
    }

    public static DateTime DtUtcNow(this TimeProvider clk) => clk.GetUtcNow().UtcDateTime;
}