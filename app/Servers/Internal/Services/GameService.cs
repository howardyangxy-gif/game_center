using app.Common;
using app.Common.Logging;
using System.Text.Json;
using app.Servers.Internal.Models;
using Mysqlx;

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
            LogService.Game.LogError("CheckTestToken", ex);
            return (false, 0, "Internal server error");
        }
    }

    /// <summary>
    /// 驗證遊戲端傳來的 userToken
    /// </summary>
    public (int errorCode, object? data, string errorMsg) CheckUserToken(CheckTokenRequest checkRequest)
    {
        try
        {
            LogService.Game.LogInfo("CheckUserToken", checkRequest);

            if (string.IsNullOrEmpty(checkRequest.token))
            {
                return ((int)ErrorCode.InvalidToken, null, "Invalid token");
            }

            var errorCode = Token.checkToken(checkRequest.token);
            if (errorCode != (int)ErrorCode.Success)
            {
                return (errorCode, null, $"Token validation failed with error code: {errorCode}");
            }


            // 從agentId 取得WalletType (暫時寫死)
            // 從agentId 取得幣種 (暫時寫死)

            return (0, new
            {
                checkRequest.agentId,
                userId = checkRequest.name,
                walletType = WalletType.Transfer,
                currency = "TWD"
            }, string.Empty);
        }
        catch (Exception ex)
        {
            LogService.Game.LogError("CheckUserToken", ex);
            return ((int)ErrorCode.SystemError, null, "Internal server error");
        }
    }

    /// <summary>
    /// 取得玩家餘額
    /// <param name="agentId">代理商ID</param>
    /// <param name="name">玩家名稱</param>
    /// <param name="walletType">錢包類型</param>
    /// <param name="currency">貨幣類型</param>
    /// <returns>是否成功, 完整資料物件, 錯誤代碼</returns>
    /// </summary>
    public (int errorCode, object? data, string errorMsg) GetPlayerBalance(BalanceRequest balanceRequest)
    {
        try
        {
            LogService.Game.LogInfo("GetPlayerBalance", balanceRequest);

            // 單一錢包, 打給代理拿餘額
            if (balanceRequest.walletType == WalletType.Single)
            {
                // 打給代理商API取得餘額 (暫時不實作)
                return ((int)ErrorCode.SingleWalletNotSupported, null, "Single wallet not supported");
            }

            // 取得玩家餘額
            var (errorCode, balance, seq) = _gameDao.GetPlayerBalance(balanceRequest.name, balanceRequest.currency);
            
            if (errorCode == 0)
            {
                var data = new
                {
                    userId = balanceRequest.name,
                    balance,
                    balanceRequest.currency,
                    seq
                };
                
                return (0, data,  string.Empty);
            }
            else
            {
                return (errorCode, null, "Player not found or balance error");
            }
        }
        catch (Exception ex)
        {
            LogService.Game.LogError("GetPlayerBalance", ex);
            return ((int)ErrorCode.SystemError, null, "Internal server error");
        }
    }

    // 玩家下注並結算
    public (int errorCode, object? data, string errorMsg) PlayerBetAndSettle(BetRequest betRequest)
    {
        try
        {
            LogService.Game.LogInfo("PlayerBetAndSettle", betRequest);

            // 分成單一錢包跟轉帳錢包
            // 單一錢包 , 通知代理商進行扣款
            // 轉帳錢包 , 錢包端扣款
            if (betRequest.walletType == WalletType.Single)
            {
                // todo:單一錢包, 會通知代理商進行扣款(下一階段再補)
                return ((int)ErrorCode.SingleWalletNotSupported, null, "單一錢包不支持此操作");
            }
            else
            {
                // todo:帳號扣款
                // 1. 先扣款
                // 2. 若失敗, 回傳錯誤, 並cancelBet?
                // 3. 若成功, 再進行結算
                // 


            }

            return ((int)ErrorCode.Success, null, "Not implemented yet");
        }
        catch (Exception ex)
        {
            LogService.Game.LogError("PlayerBetAndSettle", ex);
            return ((int)ErrorCode.SystemError, null, "Internal server error");
        }

    }
}
