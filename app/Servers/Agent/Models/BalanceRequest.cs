using app.Common;

namespace app.Servers.Agent.Models;

public class BalanceRequest : AgentRequestBase
{
    [System.Text.Json.Serialization.JsonPropertyName("acc")]
    public string name { get; set; } = string.Empty;
    public string currency { get; set; } = string.Empty;
}