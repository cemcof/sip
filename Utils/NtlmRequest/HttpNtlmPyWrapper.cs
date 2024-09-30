using System.Diagnostics;

namespace sip.Utils.NtlmRequest;

public class HttpNtlmPyOptions
{
    [Required] public string PythonExec { get; set; } = null!;
    [Required] public string NtlmScript { get; set; } = null!;
}

public class HttpNtlmPyWrapper(IOptions<HttpNtlmPyOptions> options, ILogger<HttpNtlmPyWrapper> logger)
{
    public async Task<string> GetStringAsync(string url, string username, string password)
    {
        string arguments = $"{options.Value.NtlmScript} {url} -u {username} -p {password}"; // This is kinda dangerous tho
        
        ProcessStartInfo processStartInfo = new ProcessStartInfo(options.Value.PythonExec, arguments)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using Process process = new Process();
        process.StartInfo = processStartInfo;
        logger.LogDebug("Invoking process {@ProcessInfo}", process);
        process.Start();

        // var outstream = new MemoryStream(); // This has to be disposed! 
        // var errorOutput = await process.StandardError.ReadToEndAsync();
        // var resultErr = await process.StandardError.ReadToEndAsync();
        // Debug.WriteLine(resultErr);
        var result = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        logger.LogDebug("Process finished with {ExitCode}, data length {DataLen}: {DataPreview}...", process.ExitCode, result.Length, string.Concat(result.Take(50)));
        // await process.StandardOutput.BaseStream.CopyToAsync(outstream);

        return result;
    }
}