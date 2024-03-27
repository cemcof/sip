using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace sip.Messaging;

public class MessageData
{
    public Guid Id { get; set; }

    public MimeMessage Message { get; set; } = null!;
}

public static class MimeConverters
{
    public static string MimeToStringConvert(MimeMessage message)
    {
        using var mems = new MemoryStream();
        message.WriteTo(mems);
        mems.Position = 0;
        return Encoding.UTF8.GetString(mems.ToArray());
    }

    public static MimeMessage StringToMimeConvert(string text)
    {
        using var mems = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return MimeMessage.Load(mems, persistent: false);
    }
}

public class MessageDataEntityConfiguration : IEntityTypeConfiguration<MessageData>
{
    public void Configure(EntityTypeBuilder<MessageData> builder)
    {
        builder.Property(m => m.Message)
            .HasConversion((x) => MimeConverters.MimeToStringConvert(x), x => MimeConverters.StringToMimeConvert(x));
    }
}