using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace sip.Auth.Saml2;

public class Saml2Response
{
    private readonly XmlDocument _xmlDoc;
    private readonly XmlNamespaceManager _xmlNameSpaceManager; // We need this one to run our XPath queries on the SAML XML

    public string Xml => _xmlDoc.OuterXml;

    public Saml2Response(string xmlString)
    {
        // Load XML
        _xmlDoc = new XmlDocument
        {
            PreserveWhitespace = true,
            XmlResolver = null
        };
        
        _xmlDoc.LoadXml(xmlString);
        _xmlNameSpaceManager = GetNamespaceManager(); // Support for XPath
    }

    public Saml2Response(byte[] xmlByteArray) 
        : this(Encoding.UTF8.GetString(xmlByteArray))
    { }

    public static Saml2Response FromBase64(string base64XmlString)
    {
        var xmlByteArray = Convert.FromBase64String(base64XmlString);
        return new Saml2Response(xmlByteArray);
    }

    public static string GetIssuerFromBase64Response(string base64XmlString)
    {
        var tmpRes = FromBase64(base64XmlString);
        return tmpRes.GetIssuer();
    }

    public bool IsValid(X509Certificate2 signCertificate)
    {
        var nodeList = _xmlDoc.SelectNodes("//ds:Signature", _xmlNameSpaceManager);
        if (nodeList is null || nodeList.Count == 0) return false;

        var signedXml = new SignedXml(_xmlDoc);
        signedXml.LoadXml((XmlElement)nodeList[0]!);
        return ValidateSignatureReference(signedXml) && signedXml.CheckSignature(signCertificate, true) && !IsExpired();
    }

    //an XML signature can "cover" not the whole document, but only a part of it
    //.NET's built in "CheckSignature" does not cover this case, it will validate to true.
    //We should check the signature reference, so it "references" the id of the root document element! If not - it's a hack
    private bool ValidateSignatureReference(SignedXml signedXml)
    {
        if (signedXml.SignedInfo.References.Count != 1) //no ref at all
            return false;

        var reference = (Reference)signedXml.SignedInfo.References[0]!;
        var id = reference?.Uri?[1..];

        var idElement = signedXml.GetIdElement(_xmlDoc, id);

        if (idElement == _xmlDoc.DocumentElement)
            return true;
        var assertionNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion", _xmlNameSpaceManager) as XmlElement;
        return assertionNode == idElement;
    }

    private bool IsExpired()
    {
        var expirationDate = DateTime.MaxValue;
        var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", _xmlNameSpaceManager);
        if (node?.Attributes?["NotOnOrAfter"] is not null)
        {
            if (!DateTime.TryParse(node.Attributes["NotOnOrAfter"]?.Value, out expirationDate))
                return false; // TODO
        }
        return DateTime.UtcNow > expirationDate.ToUniversalTime();
    }

    public string? GetNameId()
    {
        var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:NameID", _xmlNameSpaceManager);
        return node?.InnerText;
    }

    public string GetIssuer()
    {
        var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Issuer", _xmlNameSpaceManager);
        return node?.InnerText ?? throw new Exception("XML Response: missing Issuer");
    }

    public virtual string? GetUpn()
    {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
    }

    public virtual string? GetEmail()
    {
        return GetCustomAttribute("User.email")
               ?? GetCustomAttribute("urn:oid:0.9.2342.19200300.100.1.3")
               ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") 
               ?? GetCustomAttribute("mail");
    }

    public virtual string? GetFirstName()
    {
        return GetCustomAttribute("first_name")
               ?? GetCustomAttribute("urn:oid:2.5.4.42")
               ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname") //some providers (for example Azure AD) put last name into an attribute named "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"
               ?? GetCustomAttribute("User.FirstName")
               ?? GetCustomAttribute("givenName"); //some providers put last name into an attribute named "givenName"
    }

    public virtual string? GetLastName()
    {
        return GetCustomAttribute("last_name")
               ?? GetCustomAttribute("urn:oid:2.5.4.4")
               ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname") //some providers (for example Azure AD) put last name into an attribute named "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"
               ?? GetCustomAttribute("User.LastName")
               ?? GetCustomAttribute("sn"); //some providers put last name into an attribute named "sn"
    }


    public virtual string? GetLocation()
    {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/location")
               ?? GetCustomAttribute("physicalDeliveryOfficeName");
    }

    public string? GetCustomAttribute(string attr)
    {
        var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:AttributeStatement/saml:Attribute[@Name='" + attr + "']/saml:AttributeValue", _xmlNameSpaceManager);
        return node?.InnerText;
    }

    public string GetRequiredCustomAttribute(string attr)
    {
        var attrib = GetCustomAttribute(attr);
        if (string.IsNullOrWhiteSpace(attrib))
        {
            throw new Exception($"SAML 2 Attribute {attr} is missing");
        }

        return attrib;
    }
    
    private XmlNamespaceManager GetNamespaceManager()
    {
        var manager = new XmlNamespaceManager(_xmlDoc.NameTable);
        manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");

        return manager;
    }
}