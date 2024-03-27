
using MailKit.Net.Smtp;

namespace sip.Messaging.Email;

public class SmtpSender : IRawMessageSender
{
    private readonly IOptions<SmtpOptions> _options;
    private readonly ILogger<SmtpSender> _logger;
    private readonly SmtpClient _smtpClient = new();

    public SmtpSender(IOptions<SmtpOptions> options, ILogger<SmtpSender> logger)
    {
        _options = options;
        _logger = logger;
        _logger = logger;
    }

    public async Task SendMessage(MimeMessage message)
    {
        await SendOne(message);
        // if (!_smtpClient.IsConnected)
        // {
        //     _smtpClient.Dispose();
        //     _smtpClient = new SmtpClient();
        //     await Connect();
        // }
        //
        // var opts = _options.Value;
        // if (!_smtpClient.IsAuthenticated && opts.Login is not null)
        // {
        //     await Authenticate();
        // }
        //
        // PreprocessMessage(message);
        // _logger.LogInformation("Sending email from: {}, to: {}, cc: {}, bcc: {}, subject: {} ", 
        //     message.From, message.To, message.Cc, message.Bcc, message.Subject);
        // await _smtpClient.SendAsync(message);
    }

    public async Task SendOne(MimeMessage message)
    {
        using var smtpc = new SmtpClient();
        await Connect(smtpc);
        if (_options.Value.Login is not null)
        {
            await Authenticate(smtpc);
        }
        
        PreprocessMessage(message);
        var response = await smtpc.SendAsync(message);
        _logger.LogInformation("Sent email from: {}, to: {}, cc: {}, bcc: {}, subject: {}, \n{} ", 
            message.From, message.To, message.Cc, message.Bcc, message.Subject, response);
        await smtpc.DisconnectAsync(true);
    }

    private void PreprocessMessage(MimeMessage message)
    {
        var opts = _options.Value;
        // Setup sender and from according to options
        message.Sender = MailboxAddress.Parse(opts.From);
        message.From.Clear();
        message.From.Add(message.Sender);

        // Append receivers
        message.Bcc.AddRange( opts.AppendReceivers.Select(MailboxAddress.Parse));
        
        if (!string.IsNullOrEmpty(opts.SinkToReceiver))
        {
            message.Cc.Clear();
            message.Bcc.Clear();
            message.To.Clear();
            
            message.To.Add(MailboxAddress.Parse(opts.SinkToReceiver));
        }
        
    }

    public async Task Connect(SmtpClient? client = null)
    {
        var opts = _options.Value;
        client ??= _smtpClient;
        client.CheckCertificateRevocation = false;
        client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        _logger.LogInformation("Connecting to smtp server: {}, {}", opts.Host, opts.Port);
        await client.ConnectAsync(opts.Host, opts.Port, opts.SecureSocket);
    }

    public async Task Authenticate(SmtpClient? client = null)
    {
        client ??= _smtpClient;
        var opts = _options.Value;
        _logger.LogInformation("Authenticating to smtp server: {}", opts.Login);
        await client.AuthenticateAsync(new NetworkCredential(opts.Login,
            opts.Password));
    }

    public async Task ResendMessage(MimeMessage message, 
        IEnumerable<InternetAddress> resendTos, 
        IEnumerable<InternetAddress> resendCcs, 
        IEnumerable<InternetAddress> resendBccs)
    {
        var opts = _options.Value;
        
        message.ResentFrom.Clear();
        message.ResentReplyTo.Clear();
        message.ResentTo.Clear();
        message.ResentCc.Clear();
        message.ResentBcc.Clear();
        
        message.ResentSender = MailboxAddress.Parse(opts.From);
        message.ResentFrom.Add(message.ResentSender);
        message.ResentBcc.AddRange(resendBccs);
        message.ResentCc.AddRange(resendCcs);
        message.ResentTo.AddRange(resendTos);

        await SendOne(message);
    }
}