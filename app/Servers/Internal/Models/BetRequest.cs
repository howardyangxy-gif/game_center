using app.Common;

namespace app.Servers.Internal.Models;

// 遊戲端內部溝通使用, req包含traceID, body 包含agentId, playerId, gameCode, betAmount, winAmount, balance
// 回傳 success, balance, errorMsg 
public class BetRequest
{
    public string traceID { get; set; } = string.Empty;
    public int agentId { get; set; } = 0;

    [System.Text.Json.Serialization.JsonPropertyName("userId")]
    public string name { get; set; } = string.Empty;
    public string orderId { get; set; } = string.Empty;
    public string gameNo { get; set; } = string.Empty;
    public int gameId { get; set; } = 0;
    public int machineId { get; set; } = 0;
    public WalletType walletType { get; set; } = WalletType.Single;
    public string currency { get; set; } = string.Empty;
    public decimal bet { get; set; } = 0;
    public decimal win { get; set; } = 0;
    public decimal balance { get; set; } = 0;
}