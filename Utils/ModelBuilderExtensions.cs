using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace sip.Utils;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Modifies the model so that all DateTime and DateTime? properties
    /// has converters from/to UTC
    /// </summary>
    /// <param name="builder"></param>
    public static void UseUtc(this ModelBuilder builder)
    {
        // From: https://stackoverflow.com/questions/4648540/entity-framework-datetime-and-utc
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
        
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (entityType.IsKeyless)
            {
                continue;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }

    public static void UseStringEnums(this ModelBuilder builder)
    {
        // TODO - maybe move this to pre-convention configuration? available since ef core 6
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType.IsEnum))
            {
                var enumToStringConverter =
                    (ValueConverter)Activator.CreateInstance(typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType))!;
                property.SetValueConverter(enumToStringConverter);
            }
        }
    }
    
    // TODO - impelment timespan to string conversion
}