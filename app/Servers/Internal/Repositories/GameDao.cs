using Dapper;
using app.Infrastructure;

public class GameDao
{
    public class PlayerInfoDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Balance { get; set; }
        public int Status { get; set; }
    }

    public PlayerInfoDto? GetPlayerInfo(string playerName)
    {
        string sql = "SELECT id, name, balance, status FROM players WHERE name = @name";
        return MySqlHelper.QueryFirstOrDefault<PlayerInfoDto>(sql, new { name = playerName });
    }

    /// <summary>
    /// 更新玩家錢包餘額（未來將呼叫 SP updatePlayerWallet）
    /// </summary>
    /// <param name="name">玩家名稱</param>
    /// <param name="money">帳變金額</param>
    /// <param name="action">帳變操作</param>
    /// <param name="type">帳變類型</param>
    /// <param name="currency">幣別</param>
    /// <param name="orderNo">訂單號</param>
    /// <param name="roundNo">局號（非必填）</param>
    /// <returns>errorCode, balance, id(last_insert_id)</returns>
    public (int errorCode, decimal balance, long id) UpdatePlayerBalance(string name, decimal money, string action, string type, string currency, string orderNo, string? roundNo = null)
    {
        // TODO: 改為 call sp updatePlayerWallet
        // var result = MySqlHelper.QueryFirstOrDefault<...>("CALL updatePlayerWallet(...) ...");
        // return (result.errorCode, result.balance, result.id);
        // 目前暫用假資料
        return (0, 500m, 2L);
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
            return (1, 0, 0L);
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
            return (1, 0, 0L);
        }
    }

    

}