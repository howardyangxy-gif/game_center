using app.Common;
using System.Text.Json;

namespace app.Services;

public class GameService
{
    private readonly GameDao _gameDao = new GameDao();

    // 暫時測試token使用
    public string CreateTestToken()
    {
        var tokenData = new {
            agentId = 11000,
            playerName = "test_player",
        };

        var jsonString = JsonSerializer.Serialize(tokenData);
        var token = Token.createToken(jsonString);
        return token;
    }

    // 暫時測試token使用
    public (bool isValid, int userId, string errorMsg) CheckTestToken(string token)
    {
        try
        {
            var errorCode = Token.checkToken(token);
            if (errorCode != (int)ErrorCode.Success)
            {
                return (false, 0, $"Token validation failed with error code: {errorCode}");
            }

            // 模擬取得 userId (真實環境會從資料庫查詢)
            int userId = new Random().Next(1000, 9999); // 假設 userId 是一個隨機數字

            return (true, userId, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameService] Error in CheckTestToken: {ex.Message}");
            return (false, 0, "Internal server error");
        }
    }

    /// <summary>
    /// 驗證遊戲端傳來的 userToken
    /// </summary>
    public (bool isValid, int userId, string errorMsg) CheckUserToken(CheckTokenRequest checkRequest)
    {
        try
        {
            // log

            // 模擬驗證 token (真實環境會呼叫資料庫或其他服務)
            if (string.IsNullOrEmpty(checkRequest.token) || checkRequest.token != "exampletoken")
            {
                return (false, 0, "Invalid token");
            }

            // 模擬取得 userId (真實環境會從資料庫查詢)
            int userId = new Random().Next(1000, 9999); // 假設 userId 是一個隨機數字

            return (true, userId, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameService] Error in CheckUserToken: {ex.Message}");
            return (false, 0, "Internal server error");
        }
    }

    /// <summary>
    /// 取得玩家餘額
    /// </summary>
    public (bool success, decimal balance, string errorMsg) GetPlayerBalance(BalanceRequest balanceRequest)
    {
        try
        {
            // log

            // 取得玩家餘額
            var (errorCode, balance, id) = _gameDao.GetPlayerBalance(balanceRequest.name);
            return (true, balance, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameService] Error in GetPlayerBalance: {ex.Message}");
            return (false, 0, "Internal server error");
        }
    }

    // 玩家下注並結算
    public (bool success, decimal balance, string errorMsg) PlayerBetAndSettle(BetRequest betRequest)
    {
        try
        {
            // log 

            // 



            // 分成單一錢包跟轉帳錢包
            // 單一錢包 , 會通知代理商進行扣款
            // 轉帳錢包, 直接在錢包端扣款
            if (betRequest.walletType == WalletType.Single)
            {
                // 單一錢包, 會通知代理商進行扣款(下一階段再補)
                return (false, 0, "單一錢包不支持此操作");
            }
            else
            {
                //帳號扣款

                return (false, 0, "轉帳錢包不支持此操作");
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameService] Error in PlayerBetAndSettle: {ex.Message}");
            return (false, 0, "Internal server error");
        }

    }
}
