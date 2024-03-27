namespace sip.Forms;

public interface ICInputBase
{
    IEnumerable<string> SelfValidate(ValidationContext validationContext);
}