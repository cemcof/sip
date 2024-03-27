using System.Security.Cryptography;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

// ReSharper disable InconsistentNaming

namespace sip.RemoteAccess;

/// <summary>
/// This service implements generating tokens and urls for Guacamole external JSON authentication.
/// <see href="https://guacamole.apache.org/doc/gug/json-auth.html">https://guacamole.apache.org/doc/gug/json-auth.html</see>
/// 
/// </summary>
public class GuacamoleDriver(ILogger<GuacamoleDriver> logger, IHttpClientFactory httpClientFactory)
{
    private readonly byte[] _aesVector = new byte[16];

    public static GuacamoleAuthModel GenerateSingleConnectionAuthModel(string username, DateTime expires, string connectionName, string protocol, string hostname, int port, string password)
    {
        return new GuacamoleAuthModel(username, 
            ((DateTimeOffset) expires).ToUnixTimeMilliseconds().ToString(), 
            new Dictionary<string, GuacamoleAuthConnectionModel>
            {
                { connectionName, 
                    new GuacamoleAuthConnectConnectionModel(
                        protocol,
                        Guid.NewGuid().ToString(),
                        new Dictionary<string, object>()
                        {
                            { "hostname", hostname },
                            { "port", port },
                            { "password", password }
                        } 
                    ) }
            });
    }

    public static GuacamoleAuthModel GenerateJoinAuthModel(string id, string username, string connectionName, DateTime expires, 
        bool isReadonly)
    {
        return new GuacamoleAuthModel(username,
            ((DateTimeOffset) expires).ToUnixTimeMilliseconds().ToString(),
            new Dictionary<string, GuacamoleAuthConnectionModel>()
            {
                {
                    connectionName, new GuacamoleAuthJoinConnectionModel(id, Guid.NewGuid().ToString(),
                        new Dictionary<string, object>()
                        {
                            {"read-only", isReadonly}
                        }
                    )
                }
            });
    }
    
    public string GenerateGuacaToken(GuacamoleAuthModel guacaData, ReadOnlySpan<char> secretKey)
    {
        var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(guacaData));
        var keyBytes = secretKey.ToString().HexToByteArray(); // This is dirty, we should work with ROS
        
        // Hash JSON
        var hash = HMACSHA256.HashData(keyBytes, jsonBytes);
        var hashedJson = hash.Concat(jsonBytes).ToArray();
        
        // Encrypt hashed JSON
        using var aes = Aes.Create();
        var encryptor = aes.CreateEncryptor(keyBytes, _aesVector);
        using var memstream = new MemoryStream();
        using (var cryptostream = new CryptoStream(memstream, encryptor, CryptoStreamMode.Write))
        {
            cryptostream.Write(hashedJson);
        }
        var encryptedJson = memstream.ToArray();

        var base64Result = Convert.ToBase64String(encryptedJson);
        logger.LogDebug("Generated guaca token, expires at {} ({:G}), {} for data {}", 
              guacaData.expires, DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(guacaData.expires)), 
              base64Result, Encoding.UTF8.GetString(jsonBytes));
       
        return base64Result;
    }

    /// <summary>
    /// This is useless currently and was created mainly for testing.
    /// This just decodes token, it does not validate signature (for now)
    /// </summary>
    /// <param name="base64Token"></param>
    /// <param name="secretKey"></param>
    /// <returns></returns>
    public GuacamoleAuthModel DecodeGuacaToken(string base64Token, ReadOnlySpan<char> secretKey)
    {
        var tokenBytes = Convert.FromBase64String(base64Token);
        var keyBytes = secretKey.ToString().HexToByteArray();

        // Decrypt
        using var aes = Aes.Create();
        var decryptor = aes.CreateDecryptor(keyBytes, _aesVector);
        using var memstream = new MemoryStream(tokenBytes);
        using var decryptedMs = new MemoryStream();
        using (var cryptostream = new CryptoStream(memstream, decryptor, CryptoStreamMode.Read))
        {
            cryptostream.CopyTo(decryptedMs);
        }

        var decryptedBytes = decryptedMs.ToArray();
        
        // Dehash
        // var hash = decryptedJson.Take(32).ToArray().AsSpan();
         
        var dehashedJson = decryptedBytes.Skip(32).ToArray();
        // var hashCheck = HMACSHA256.HashData(keyBytes, Encoding.UTF8.GetBytes(dehashedJson.ToArray())).AsSpan();
          
        
        var result = JsonSerializer.Deserialize<GuacamoleAuthModel>(dehashedJson);
        if (result is null) throw new JsonException("JSON deserializer result is null");
        return result;
    }

    public string GenerateGuacaUrlFromToken(string baseUrl, string guacaSessionToken)
    {
        guacaSessionToken = WebUtility.UrlEncode(guacaSessionToken);
        var url = baseUrl + $"/?token={guacaSessionToken}";
        return url;
    }
    

    /// <summary>
    /// Opens guacamole session with given data via HTTP Post request
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="guacamoleToken"></param>
    /// <returns></returns>
    public async Task<string> SessionConnect(string baseUrl, string guacamoleToken)
    {
        using var httpc = httpClientFactory.CreateClient();
        var apiUrl = baseUrl + "/api/tokens";
        
        try
        {
            var response = await httpc.PostAsync(apiUrl, new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("data", guacamoleToken)}));
            logger.LogDebug("Response ({}) {}", response.StatusCode, await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GuacaNewTokenReponse>();
            if (string.IsNullOrEmpty(result?.authToken))
                throw new InvalidOperationException("Cannot find authentication token in response body");

            return result.authToken;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Opening guacamole session failed");
            throw;
        }
    }

    public async Task RemoveSessionIfExists(string baseUrl, string guacamoleToken)
    {
        using var httpc = httpClientFactory.CreateClient();
        
        try
        {
            var targetUrl = baseUrl +
                            $"/api/session/data/json/connections?token={guacamoleToken}"; // TODO -encode?
            var res = await httpc.GetAsync(targetUrl);
            if (res.IsSuccessStatusCode)
            {
                logger.LogInformation("Removing session {}", guacamoleToken);
                var targetUrlDel = baseUrl + $"/api/tokens/{guacamoleToken}";
                await httpc.DeleteAsync(targetUrlDel);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during guaca periodic handling, skipping rest of the round");
        }
    }
}