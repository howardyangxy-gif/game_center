using System.Text;
using System.Text.Json;

namespace app.Common;

public static class Token
{
    const string SecretKey = "ESD1235_#33WCKSN"; // 密鑰

    public static string createToken(string data, int expireSeconds = 150)
    {
        var payload = new
        {
            data = data,
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddSeconds(expireSeconds).ToUnixTimeSeconds()
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));
        
        var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
        var signature = Convert.ToBase64String(hash);

        return  $"{payloadBase64}.{signature}";
    }

    public static string decodeToken(string token)
    {
        var base64EncodedBytes = Convert.FromBase64String(token);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static int checkToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return (int)ErrorCode.InvalidToken;

            var tokenParts = token.Split('.');
            if (tokenParts.Length != 2)
                return (int)ErrorCode.InvalidToken;

            // 驗證 token 簽名
            var payloadBase64 = tokenParts[0];
            var signature = tokenParts[1];

            var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
            var expectedSignature = Convert.ToBase64String(expectedHash);

            if (signature != expectedSignature)
                return (int)ErrorCode.InvalidSignature;

            string payloadJson;
            try
            {
                var payloadBytes = Convert.FromBase64String(payloadBase64);
                payloadJson = Encoding.UTF8.GetString(payloadBytes);
            }
            catch
            {
                return (int)ErrorCode.InvalidToken;
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(payloadJson);
            if (tokenData.ValueKind == JsonValueKind.Undefined)
                return (int)ErrorCode.InvalidToken;
                                                    
            // 檢查是否過期
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            if (!tokenData.TryGetProperty("exp", out var expProperty))
                return (int)ErrorCode.InvalidToken;
                
            var expTime = expProperty.GetInt64();
            
            if (currentTime > expTime)
                return (int)ErrorCode.TokenExpired;
            
            return (int)ErrorCode.Success;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Token] JSON parsing error: {ex.Message}");
            return (int)ErrorCode.InvalidToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Token] System error: {ex.Message}, StackTrace: {ex.StackTrace}");
            return (int)ErrorCode.SystemError;
        }
    }


} 