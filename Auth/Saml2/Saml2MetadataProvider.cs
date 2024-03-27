using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace sip.Auth.Saml2;
// TODO - validate IDP metadata xml
// TODO - do not download multiple times

public class Saml2Metadata(string entityId, string singleSignOnDestination, List<X509Certificate2> signInCerts)
{
    public const string BINDING_REDIRECT = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect";
    public const string BINDING_POST = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";

    public string                 EntityId                { get; private set; } = entityId;
    public string                 SingleSignOnDestination { get; private set; } = singleSignOnDestination;
    public List<X509Certificate2> SignInCerts             { get; }              = signInCerts;
}

public interface ISaml2MetadataProvider
{
    Task<Saml2Metadata> GetMetadata(Saml2AuthenticationOptions saml2AuthenticationOptions, string? idpEntityId = null);
}

public class Saml2MetadataProvider(ILogger<Saml2MetadataProvider> logger, IHttpClientFactory clientFactory)
    : ISaml2MetadataProvider
{
    private readonly ILogger<Saml2MetadataProvider> _logger        = logger;

    private Dictionary<string, Saml2Metadata>? _metadataCache;
    
    public async Task<Saml2Metadata> GetMetadata(Saml2AuthenticationOptions options, string? entityId = null)
    {
        // Metadata not cached, get them, parse them, cache them.
        if (_metadataCache is null)
        {
            await ParseMetadataToCache(options, true);
        }
        
        entityId ??= _metadataCache!.Keys.First();

        if (_metadataCache!.ContainsKey(entityId))
        {
            return _metadataCache[entityId];
        }

        throw new InvalidOperationException("Cannot get metadata for " + entityId);
    }

    private async Task DownloadRawMetadata(Saml2AuthenticationOptions options, CancellationToken cancellationToken)
    {
        using var httpc = clientFactory.CreateClient();

        await using var stream = await httpc.GetStreamAsync(options.IdpMetadataUrl, cancellationToken);
        Directory.CreateDirectory(options.DataDirectory);
        var targetPath = Path.Combine(options.DataDirectory, options.IdpMetaCacheFilename);
        await using var targetStream = File.OpenWrite(targetPath);
        await stream.CopyToAsync(targetStream, cancellationToken);
    }

    private async Task<StreamReader> GetRawMetadata(Saml2AuthenticationOptions options)
    {
        if (!File.Exists(options.IdpMetaCachePath))
        {
            await DownloadRawMetadata(options, CancellationToken.None);
        }

        return new StreamReader(options.IdpMetaCachePath);
    }

    private async Task ParseMetadataToCache(Saml2AuthenticationOptions options, bool validate)
    {
        using var metaRaw = await GetRawMetadata(options);
        var xml = new XmlDocument();
        xml.Load(metaRaw);
        var xmlns = GetSamlXmlIdpMetaNamespaceManager(xml);
        
        // Find all entity descriptor elements
        var entdescs = xml.SelectNodes("//md:EntityDescriptor", xmlns);
        var newMetaCache = new Dictionary<string, Saml2Metadata>();
        foreach (XmlNode entdesc in entdescs ?? throw new InvalidDataException("Invalid IDP metadata XML, no md:EntityDescriptor found"))
        {
            var entityid = entdesc.Attributes?["entityID"]?.Value ??
                           throw new InvalidDataException("Invalid IDP metadata XML, no entityID attribute");

            var targetUrl = entdesc.SelectSingleNode($"descendant::md:SingleSignOnService[@Binding='{Saml2Metadata.BINDING_REDIRECT}']/@Location", xmlns)?.Value ??
                            throw new InvalidDataException("Invalid IDP metadata XML, no Location attribute");
            // TODO - Instead of throwing, just warn and skip the entity descriptor
            
            
            // Parse signing cert
            var keyDesc = entdesc.SelectNodes("descendant::md:KeyDescriptor[contains(@use,'signing') or not(@use)]", xmlns);
            if (keyDesc is null || keyDesc.Count == 0)
            {
                throw new InvalidDataException("No signature found.");
            }
            
            var certs = ReadKeyDescriptorElements(keyDesc);
            
            newMetaCache[entityid] = new Saml2Metadata(entityid, targetUrl, certs.ToList());
        }

        _metadataCache = newMetaCache;
    }
    
    private static XmlNamespaceManager GetSamlXmlIdpMetaNamespaceManager(XmlDocument xmlDocument)
    {
        var manager = new XmlNamespaceManager(xmlDocument.NameTable);
        manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        manager.AddNamespace("md", "urn:oasis:names:tc:SAML:2.0:metadata");

        return manager;
    }
    
    // From ITFOXTEC
    protected IEnumerable<X509Certificate2> ReadKeyDescriptorElements(
        XmlNodeList keyDescriptorElements)
    {
        foreach (XmlNode descriptorElement in keyDescriptorElements)
        {
            if (descriptorElement.SelectSingleNode("*[local-name()='KeyInfo']") is XmlElement xmlElement)
            {
                var keyInfo = new KeyInfo();
                keyInfo.LoadXml(xmlElement);
                foreach (var obj in keyInfo)
                {
                    if (obj is KeyInfoX509Data keyInfoX509Data)
                    {
                        foreach (var certificate in keyInfoX509Data.Certificates)
                        {
                            if (certificate is X509Certificate2 cert)
                                yield return cert;
                        }
                    }
                }
            }
        }
    }
}
