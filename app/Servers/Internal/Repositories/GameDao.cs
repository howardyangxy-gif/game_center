using Dapper;
using app.Common;
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
    /// 儲存遊戲注單
    /// </summary>
    /// <param name="orderId">局號</param>
    /// <param name="gameId">遊戲ID</param>
    /// <param name="agentId">代理ID</param>
    /// <param name="name">玩家名稱</param>
    /// <param name="bet">下注金額</param>
    /// <param name="win">獲利金額</param>
    /// <param name="cellScore">流水</param>
    /// <param name="revenue">抽水</param>
    /// <param name="record">注單資訊</param>
    /// <param name="currency">幣別</param>
    /// <param name="gameEndTimeMs">遊戲結束時間（Unix 毫秒時間戳）</param>
    /// <param name="reason">備註</param>
    /// <returns>errorCode, string errorMsg</returns>
    public (int errorCode, string errorMsg) SaveGameRecord(
        string orderId, int gameId, int agentId, string name, 
        decimal bet, decimal win, decimal cellScore, decimal revenue, 
        string record, string currency, long gameEndTimeMs, string? reason = null)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        try
        {
            conn.Open();
            
            // 取得遊戲代碼
            string? gameCode = conn.QueryFirstOrDefault<string>(
                "SELECT gameCode FROM game_info WHERE gameId = @gameId", 
                new { gameId });
                
            if (string.IsNullOrEmpty(gameCode))
            {
                return ((int)ErrorCode.GameNotFound, $"Game ID {gameId} not found");
            }
            
            // 將 timestamp 轉換為 DateTime
            DateTime gameEndTime = DateTimeOffset.FromUnixTimeMilliseconds(gameEndTimeMs).DateTime;
            
            // 建立動態表名
            string tableName = $"game_record_{gameCode}";
            
            // 插入注單記錄
            string sql = $@"
                INSERT INTO {tableName} 
                (orderId, gameId, agentId, account, bet, win, 
                 cellScore, revenue, record, currency, reason, gameEndTime, createTime)
                VALUES 
                (@orderId, @gameId, @agentId, @name, @bet, @win,
                 @cellScore, @revenue, @record, @currency, @reason, @gameEndTime, NOW())";

            var parameters = new
            {
                orderId,
                gameId,
                agentId,
                name,
                bet,
                win,
                cellScore,
                revenue,
                record,
                currency,
                reason,
                gameEndTime
            };
            
            int affectedRows = conn.Execute(sql, parameters);
            
            if (affectedRows > 0)
            {
                return (0, string.Empty);
            }
            else
            {
                return ((int)ErrorCode.GameRecordInsertError, "Failed to insert game record");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SaveGameRecord] Exception: {ex.Message}\n{ex.StackTrace}");
            return ((int)ErrorCode.DatabaseError, $"Database error: {ex.Message}");
        }
    }

    /// <summary>
    /// 取得玩家錢包餘額    
    /// </summary>
    /// <param name="name">玩家名稱</param>
    /// <returns>errorCode, balance, seq</returns> 
    /// errorCode: 0=成功, 1=例外錯誤, 2=name不存在
    public (int errorCode, decimal balance, long seq) GetPlayerBalance(string name, string currency)
    {
        using var conn = new MySql.Data.MySqlClient.MySqlConnection(MySqlHelper.GetConnStr());
        try
        {
            var result = conn.QueryFirstOrDefault("SELECT money, seq FROM player_wallets WHERE name = @name and currency = @currency", new { name, currency });
            if (result == null)
                return ((int)ErrorCode.PlayerNotFound, 0, 0L);
            return (0, (decimal)result.money, (long)result.seq);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetPlayerBalance] Exception: {ex.Message}\n{ex.StackTrace}");
            return ((int)ErrorCode.DatabaseError, 0, 0L);
        }
    }

    

}