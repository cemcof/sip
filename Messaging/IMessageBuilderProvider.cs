namespace sip.Messaging;

public interface IMessageBuilderProvider<out TBuilder>
{
    TBuilder CreateBuilder();
}