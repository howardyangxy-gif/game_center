
public class CheckTokenRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("userId")]
    public string name { get; set; } = string.Empty;
    public string agentId { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
}