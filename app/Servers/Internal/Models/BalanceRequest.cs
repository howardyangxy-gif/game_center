using app.Common;

namespace app.Servers.Internal.Models;

public class BalanceRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("userId")]
    public string name { get; set; } = string.Empty;
    public int agentId { get; set; } = 0;
    public WalletType walletType { get; set; } = WalletType.Single;
    public string currency { get; set; } = string.Empty;
}