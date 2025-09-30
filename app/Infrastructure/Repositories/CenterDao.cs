using Dapper;
using app.Infrastructure;
using Microsoft.OpenApi.Models;

namespace app.Infrastructure.Repositories;

public class CenterDao
{


    public class WalletOperationResult
    {
        public int ErrCode { get; set; }
        public decimal Money { get; set; }
        public long WalletSeq { get; set; }
    }

    /// <summary>
    /// 更新代理商錢包餘額（呼叫 SP updateAgentWallet）
    /// </summary>
    /// <param name="agentId">代理商 ID</param>
    /// <param name="money">帳變金額</param>
    /// <param name="action">帳變操作</param>
    /// <param name="type">帳變類型</param>
    /// <param name="currency">幣別</param>
    /// <param name="orderId">訂單號</param>
    /// <param name="memo">備註（非必填）</param>
    /// <returns>errorCode, balance, id(last_insert_id)</returns>
    public (int errorCode, decimal balance, long id) UpdateAgentBalance(int agentId, decimal money, int action, int type, string currency, string orderId, string? memo = null)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        try
        {
            var parameters = new
            {
                in_agentId = agentId,
                in_money = money,
                in_action = action,
                in_type = type,
                in_orderId = orderId,
                in_memo = memo
            };
            //log一下
            Console.WriteLine($"[UpdateAgentBalance] Parameters: {System.Text.Json.JsonSerializer.Serialize(parameters)}");
            conn.Open();
            var result = conn.QueryFirstOrDefault<WalletOperationResult>(
                "CALL updateAgentWallet(@in_agentId, @in_money, @in_action, @in_type, @in_orderId, @in_memo)",
                parameters
            );

            if (result == null)
                return (9001, 0m, 0L); // SP執行成功但無返回結果

            return (result.ErrCode, result.Money / 100.0m, result.WalletSeq);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateAgentBalance] Exception: {ex.Message}\n{ex.StackTrace}");
            return (9003, 0m, 0L); // 資料庫執行錯誤
        }
    }

    /// <summary>
    /// 更新玩家錢包餘額（未來將呼叫 SP updatePlayerWallet）
    /// </summary>
    /// <param name="name">玩家名稱</param>
    /// <param name="money">帳變金額</param>
    /// <param name="action">帳變操作</param>
    /// <param name="type">帳變類型</param>
    /// <param name="currency">幣別</param>
    /// <param name="orderId">訂單號</param>
    /// <param name="memo">備註（非必填）</param>
    /// <returns>errorCode, balance, id(last_insert_id)</returns>
    public (int errorCode, decimal balance, long id) UpdatePlayerBalance(string name, decimal money, int action, int type, string currency, string orderId, string? memo = null)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        try
        {
            var parameters = new
            {
                in_name = name,
                in_money = money,
                in_action = action,
                in_type = type,
                in_currency = currency,
                in_orderId = orderId,
                in_memo = memo
            };
            //log一下
            Console.WriteLine($"[UpdatePlayerBalance] Parameters: {System.Text.Json.JsonSerializer.Serialize(parameters)}");

            conn.Open();
            var result = conn.QueryFirstOrDefault<WalletOperationResult>(
                "CALL updatePlayerWallet(@in_name, @in_money, @in_action, @in_type, @in_currency, @in_orderId, @in_memo)",
                parameters
            );

            if (result == null)
                return (9001, 0m, 0L); // SP執行成功但無返回結果

            return (result.ErrCode, result.Money / 100.0m, result.WalletSeq);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdatePlayerBalance] Exception: {ex.Message}\n{ex.StackTrace}");
            return (9003, 0m, 0L); // 資料庫執行錯誤
        }
    }

    /// <summary>
    /// 更新代理商錢包餘額（SQL 實作，回傳更新後餘額）
    /// </summary>
    public (int errorCode, decimal balance, long id) UpdateAgentBalanceSql(int agentId, decimal money)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            conn.Execute("UPDATE agent_wallets SET money = money + @money WHERE agentId = @agentId", new { money, agentId }, tran);
            var balance = conn.QueryFirstOrDefault<decimal>("SELECT money FROM agent_wallets WHERE agentId = @agentId", new { agentId }, tran);
            tran.Commit();
            return (0, balance, 0L);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            Console.WriteLine($"[UpdateAgentBalanceSql] Exception: {ex.Message}\n{ex.StackTrace}");
            return (9003, 0, 0L); // 資料庫執行錯誤
        }
    }

    /// <summary>
    /// 更新玩家錢包餘額（SQL 實作，回傳更新後餘額）
    /// </summary>
    public (int errorCode, decimal balance, long id) UpdatePlayerBalanceSql(string name, decimal money)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int affectedRows = conn.Execute("UPDATE player_wallets SET money = money + @money WHERE name = @name", new { money, name }, tran);
            if (affectedRows == 0)
            {
                tran.Rollback();
                return (2, 0, 0L); // 2: name 不存在或沒異動
            }
            var balance = conn.QueryFirstOrDefault<decimal>("SELECT money FROM player_wallets WHERE name = @name", new { name }, tran);
            tran.Commit();
            return (0, balance, 0L);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            Console.WriteLine($"[UpdatePlayerBalanceSql] Exception: {ex.Message}\n{ex.StackTrace}");
            return (9003, 0, 0L); // 資料庫執行錯誤
        }
    }

    /// <summary>
    /// 取得玩家錢包餘額    
    /// </summary>
    /// <param name="name">玩家名稱</param>
    /// <returns>errorCode, balance, id(last_insert_id)</returns> 
    /// errorCode: 0=成功, 1=例外錯誤, 2=name不存在
    public (int errorCode, decimal balance, long id) GetPlayerBalance(string name)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        try
        {
            var balance = conn.QueryFirstOrDefault<decimal?>("SELECT money FROM player_wallets WHERE name = @name", new { name });
            if (balance == null)
                return (2, 0, 0L); // 2: name 不存在
            return (0, balance.Value, 0L);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetPlayerBalance] Exception: {ex.Message}\n{ex.StackTrace}");
            return (9003, 0, 0L); // 資料庫執行錯誤
        }
    }

    public (int affectedRows, string err) UpdateAccount(int agentId, string playerName)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int affectedRows = conn.Execute("UPDATE players SET lastLoginTime = NOW() WHERE agentId = @agentId AND name = @playerName", new { agentId, playerName }, tran);
            tran.Commit();
            return (affectedRows, string.Empty);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            Console.WriteLine($"[UpdateAccount] Exception: {ex.Message}\n{ex.StackTrace}");
            return (-1, ex.Message);
        }
    }

    public void CreatePlayer(int agentId, string playerName)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            conn.Execute("INSERT INTO players (agentId, name, createTime, lastLoginTime) VALUES (@agentId, @playerName, NOW(), NOW())", new { agentId, playerName }, tran);
            // 同時在 player_wallets 建立對應的錢包紀錄，初始餘額為 0
            conn.Execute(@"INSERT INTO player_wallets (name, currency, money)
                            VALUES (@playerName,
                        (SELECT currency FROM agents WHERE id = @agentId),0)", new { agentId, playerName }, tran);
            tran.Commit();
        }
        catch (Exception ex)
        {
            tran.Rollback();
            Console.WriteLine($"[CreatePlayer] Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }
}