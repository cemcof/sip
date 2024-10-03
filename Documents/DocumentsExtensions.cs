using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using sip.Core;
using sip.Documents.Proposals;
using sip.Documents.Renderers;
using sip.Documents.Renderers.Handlebars;
using sip.Documents.Renderers.Pdf;
using sip.Documents.Renderers.Word;

namespace sip.Documents;

public class DocumentTypeBuilder<TDocumentType>(IServiceCollection services)
    where TDocumentType : Document
{
    private readonly IServiceCollection _services = services;
}

public class DocumentsBuilder
{
    private readonly IServiceCollection _services;

    public DocumentsBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;

        _services.TryAddSingleton<DocumentService>();
        _services.TryAddSingleton<ZipArchiver>();
        _services.TryAddSingleton<EmbeddedFileProvider>(_ => new EmbeddedFileProvider(Assembly.GetExecutingAssembly()));

        // Add renderers 
        _services.TryAddSingleton<MsWordRenderer>();
        _services.AddSingleton<IDocRenderer, MsWordRenderer>();

        _services.TryAddSingleton<WeasyPrintPdfRenderer>();
        _services.Configure<WeasyPrintPdfRendererOptions>(configuration.GetSection("WeasyPrintPdf"));
        _services.AddSingleton<IDocRenderer, WeasyPrintPdfRenderer>();
        _services.TryAddSingleton<IDocumentProvider, DocumentProviderDelegator>();

        AddHandlebars();

        _services.ConfigureDbModel(c =>
        {
            c.Entity<Document>();
            c.Entity<Proposal>();
            c.Entity<FileInDocument>();
            new FileMetadataEntityTypeConfiguration().Configure(c.Entity<FileMetadata>());
            c.Entity<FileData>();
        });
        
        // Add self http client
        services.AddHttpClient<DocSelfHttp>((sp, httpc) =>
        {
            var opts = sp.GetRequiredService<IOptions<AppOptions>>().Value;
            httpc.BaseAddress = opts.UrlBaseLocal;
        })
        .ConfigurePrimaryHttpMessageHandler(_ =>
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });

    }

    public DocumentsBuilder AddHandlebars(Action<HandlebarsBuilder>? buildHandlebars = null)
    {
        var handlebarsBuilder = new HandlebarsBuilder(_services);
        buildHandlebars ??= _ => { };
        buildHandlebars(handlebarsBuilder);
        return this;
    }
    
    public DocumentsBuilder AutoScanDocuments(Assembly assembly)
    {
        var proposalTypes = assembly.FindImplementations<Document>();
        foreach (var proposalType in proposalTypes)
        {
            var meth = GetType().GetMethod(nameof(AutoAddDocument))!.MakeGenericMethod(proposalType);
            meth.Invoke(this, null);
        }

        return this;
    }
    
    public DocumentsBuilder AutoAddDocument<TDocument>() where  TDocument : Document
    {
        // Automatically add implementations of several interfaces
        _services.Scan(s =>
        {
            s.FromAssemblyOf<TDocument>()
                .AddClasses(c =>
                {
                    c.AssignableTo<IDocumentFactory<TDocument>>();
                    // c.AssignableTo<IProposalSubmiter<TDocument>>();
                    c.AssignableTo<IDocumentRenderInfoProvider<TDocument>>();
                })
                .AsSelfWithInterfaces()
                .WithSingletonLifetime();
        });
        
        // Add to db model
        _services.ConfigureDbModel(c => c.Entity<TDocument>());
        return this;
    }
}