namespace sip.Environment;

public class EnvironmentService(ILogger<EnvironmentService> logger)
{
    public           EnvironmentModel?           Data { get; private set; }
    public event Action?                         Refresh;

    public void RcvPapouch(EnvironmentModel data)
    {
        logger.LogDebug("Received environment model data: \n{}", data);
        Data = data;
        Refresh?.Invoke();
    }
}