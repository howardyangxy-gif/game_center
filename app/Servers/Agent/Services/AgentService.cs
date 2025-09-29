using System.Text.Json;
using app.Common;
using app.Common.Logging;
using app.Infrastructure.Repositories;
using app.Servers.Agent.Models;

namespace app.Servers.Agent.Services;

public class AgentService
{
    private readonly AgentDao _agentDao = new AgentDao();
    private readonly CenterDao _centerDao = new CenterDao();

    public string GetAesKeyForAgent(int agentId)
    {
        var info = _agentDao.GetAgentInfo(agentId);
        return info?.AesKey ?? string.Empty;
    }

    public string GetHmacKeyForAgent(int agentId)
    {
        var info = _agentDao.GetAgentInfo(agentId);
        return info?.HmacKey ?? string.Empty;
    }

    public AgentDao.AgentInfoDto? GetAgentInfo(int agentId)
    {
        return _agentDao.GetAgentInfo(agentId);
    }

    // <summary>
    // 玩家登入，成功回傳 遊戲連結
    // </summary>
    public (bool success, string gameUrl, string errorMsg) PlayerLogin(LoginRequest loginRequest)
    {
        LogService.Agent.LogInfo("Player login request received", new { 
            AgentId = loginRequest.agentId, 
            PlayerName = loginRequest.name,
            GameId = loginRequest.gameId,
            Currency = loginRequest.currency,
            Lang = loginRequest.lang
        });
        
        loginRequest.name = loginRequest.agentId.ToString() + '_' + loginRequest.name.Trim();

        // todo:檢查幣種是否與代理幣種一致(下一階段再補)

        // 更新玩家信息，不存在則創建
        var (result, err) = UpdateAccount(loginRequest.agentId, loginRequest.name);
        if (result != 0)
        {
            return (false, string.Empty, $"更新或創建玩家失敗: {err}");
        }

        var token = Token.createToken(JsonSerializer.Serialize(new {
            agentId = loginRequest.agentId,
            playerName = loginRequest.name,
        }));

        // todo:產生遊戲連結(下一階段再補)
        // 分為大廳跟直接近遊戲

        // 遊戲連結
        string gameUrl = $"https://game.example.com/play?user={loginRequest.name}&lang={loginRequest.lang}&currency={loginRequest.currency}&gameId={loginRequest.gameId}&token={token}&BackUrl={Uri.EscapeDataString(loginRequest.backUrl)}";

        return (true, gameUrl, string.Empty);
    }
    public (bool success, decimal balance, string errorMsg) PlayerDeposit(TransferRequest transferRequest)
    {
        Console.WriteLine($"[AgentService] Player deposit request: {JsonSerializer.Serialize(transferRequest)}");
        // 檢查錢包type, 單一錢包則略過
        if (transferRequest.walletType == WalletType.Single)
        {
            // 單一錢包, 直接回傳失敗狀態碼
            return (false, 0, "單一錢包不支持此操作");
        }

        // 判斷金額正常與否
        if (transferRequest.amount <= 0)
        {
            return (false, 0, "金額必須大於0");
        }

        transferRequest.name = transferRequest.agentId.ToString() + '_' + transferRequest.name.Trim();

        // redis檢查, 確認該玩家否正在上下分(下一階段再補)
        // orderid檢查, 確認該訂單是否已處理過(下一階段再補)

        // 1. 代理下分
        var (agentUpdateResult, agentBalanceNew, agentSeqNew) = _centerDao.UpdateAgentBalance(transferRequest.agentId, transferRequest.amount,
            (int)WalletAction.PlayerDeposit, (int)WalletTransactionType.TransferDeposit, transferRequest.currency, transferRequest.orderId);
        if (agentUpdateResult != 0)
        {
            return (false, 0, $"代理商餘額更新失敗: {CommonUtils.GetErrorMessage(agentUpdateResult)}");
        }

        Console.WriteLine($"[AgentService] Agent {transferRequest.agentId} balance decreased to {agentBalanceNew}");
        // 2. 玩家上分
        var (playerUpdateResult, playerBalanceNew, playerSeqNew) = _centerDao.UpdatePlayerBalance(transferRequest.name, transferRequest.amount,
            (int)WalletAction.PlayerDeposit, (int)WalletTransactionType.TransferDeposit, transferRequest.currency, transferRequest.orderId);
        if (playerUpdateResult != 0)
        {
            // 玩家上分失敗, 需要補回代理商的錢
            var (rollBackResult, rollBackBalance, agentRollBackSeqNew) = _centerDao.UpdateAgentBalance(transferRequest.agentId, transferRequest.amount
                , (int)WalletAction.PlayerDepositRollback, (int)WalletTransactionType.TransferDepositRollback, transferRequest.currency, $"RB_{transferRequest.orderId}");
            if (rollBackResult != 0)
            {
                return (false, 0, $"玩家上分失敗 回滾代理金額失敗: {CommonUtils.GetErrorMessage(playerUpdateResult)}, Rollback agent failed: {CommonUtils.GetErrorMessage(rollBackResult)}");
            }
            return (false, 0, $"玩家上分失敗 回滾代理金額成功: {CommonUtils.GetErrorMessage(playerUpdateResult)}");
        }

        Console.WriteLine($"[AgentService] Player {transferRequest.name} balance increased to {playerBalanceNew}");

        // 3. 查詢玩家最新餘額
        decimal balance = playerBalanceNew;

        return (true, balance, string.Empty);
    }

    public (bool success, decimal balance, string errorMsg) PlayerWithdrawal(TransferRequest transferRequest)
    {
        Console.WriteLine($"[AgentService] Player withdrawal request: {JsonSerializer.Serialize(transferRequest)}");
        // 檢查錢包type, 單一錢包則略過
        if (transferRequest.walletType == WalletType.Single)
        {
            // 單一錢包, 直接回傳失敗狀態碼
            return (false, 0, "單一錢包不支持此操作");
        }

        // 判斷金額正常與否
        if (transferRequest.amount <= 0)
        {
            return (false, 0, "金額必須大於0");
        }

        transferRequest.name = transferRequest.agentId.ToString() + '_' + transferRequest.name.Trim();

        // redis檢查, 確認該玩家否正在上下分(下一階段再補)
        // orderid檢查, 確認該訂單是否已處理過(下一階段再補)

        // 1. 玩家下分
        var (playerUpdateResult, playerBalanceNew, playerSeqNew) = _centerDao.UpdatePlayerBalance(transferRequest.name, transferRequest.amount,
            (int)WalletAction.PlayerWithdraw, (int)WalletTransactionType.TransferWithdraw, transferRequest.currency, transferRequest.orderId);
        if (playerUpdateResult != 0)
        {
            return (false, 0, $"玩家下分失敗: {CommonUtils.GetErrorMessage(playerUpdateResult)}");
        }

        Console.WriteLine($"[AgentService] Player {transferRequest.name} balance decreased to {playerBalanceNew}");
        
        // 2. 代理上分
        var (agentUpdateResult, agentBalanceNew, agentSeqNew) = _centerDao.UpdateAgentBalance(transferRequest.agentId, transferRequest.amount,
            (int)WalletAction.PlayerWithdraw, (int)WalletTransactionType.TransferWithdraw, transferRequest.currency, transferRequest.orderId);
        if (agentUpdateResult != 0)
        {
            // 代理商上分失敗, 需要補回玩家的錢
            var (rollBackResult, rollBackBalance, playerRollBackSeqNew) = _centerDao.UpdatePlayerBalance(transferRequest.name, transferRequest.amount,
                (int)WalletAction.PlayerWithdrawRollback, (int)WalletTransactionType.TransferWithdrawRollback, transferRequest.currency, $"RB_{transferRequest.orderId}");
            if (rollBackResult != 0)
            {
                return (false, 0, $"代理商上分失敗 回滾玩家金額失敗: {CommonUtils.GetErrorMessage(agentUpdateResult)}, Rollback player failed: {CommonUtils.GetErrorMessage(rollBackResult)}");
            }
            return (false, 0, $"代理商上分失敗 回滾玩家金額成功: {CommonUtils.GetErrorMessage(agentUpdateResult)}");
        }

        Console.WriteLine($"[AgentService] Agent {transferRequest.agentId} balance increased to {agentBalanceNew}");

        // 3. 查詢玩家最新餘額
        decimal balance = playerBalanceNew;

        return (true, balance, string.Empty);
    }

    public (bool success, decimal balance, string errorMsg) GetPlayerBalance(BalanceRequest balanceRequest)
    {
        Console.WriteLine($"[AgentService] Get player balance request: {JsonSerializer.Serialize(balanceRequest)}");
        // 檢查錢包type, 單一錢包則略過
        if (balanceRequest.walletType == WalletType.Single)
        {
            // 單一錢包, 直接回傳失敗狀態碼
            return (false, 0, "單一錢包不支持此操作");
        }

        balanceRequest.name = balanceRequest.agentId.ToString() + "_" + balanceRequest.name.Trim();
        
        // 1. 查詢玩家餘額  
        var (Result, playerBalance, Err) = _centerDao.GetPlayerBalance(balanceRequest.name);
        if (Result != 0)
            return (false, 0, $"玩家查詢餘額失敗: {Err}");
        return (true, playerBalance, string.Empty);
    }

    public (int result, string err) UpdateAccount(int agentId, string playerName)
    {
        try
        {
            // 更新玩家最後登入時間, 不存在則創建
            var (Result, Err) = _centerDao.UpdateAccount(agentId, playerName);
            if (Result == -1)
            {
                return (1, $"更新玩家最後登入時間失敗: {Err}");
            }
            else if (Result == 0)
            {
                // 創建新玩家
                _centerDao.CreatePlayer(agentId, playerName);
                Console.WriteLine($"[AgentService] Created new player: {playerName} for agent {agentId}");
            }

            return (0, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AgentService] Error in UpdateAccount transaction: {ex.Message}");
            return (1, "Database error during account update");
        }
    }
}