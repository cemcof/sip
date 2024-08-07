using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace sip.Messaging;

public enum MessageRecipientType
{
    Primary, Copy, BlindCopy
}

public class MessageRecipient
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public MessageRecipientType Type { get; set; }
}

public class MessageRecipientEntityConfig : IEntityTypeConfiguration<MessageRecipient>
{
    public void Configure(EntityTypeBuilder<MessageRecipient> builder)
    {
        builder.Property(p => p.Type)
            .HasConversion<string>();
        
    }
}