using Dapper;
using app.Infrastructure;
using Microsoft.OpenApi.Models;

public class AgentDao
{
    public class AgentInfoDto
    {
        public int Id { get; set; }
        public string? AesKey { get; set; }
        public string? HmacKey { get; set; }
        public int WalletType { get; set; }
        public string? Currency { get; set; }
        public int Status { get; set; }
        public string? whiteIp { get; set; } // 新增 whiteIp 屬性
    }

    public class WalletOperationResult
    {
        public int ErrCode { get; set; }
        public decimal Money { get; set; }
        public long WalletSeq { get; set; }
    }

    public AgentInfoDto? GetAgentInfo(int agentId)
    {
        string sql = "SELECT id, aesKey, hmacKey, walletType, currency, whiteIp, status FROM agents WHERE id = @id";
        return MySqlHelper.QueryFirstOrDefault<AgentInfoDto>(sql, new { id = agentId });
    }

    public bool UpdateAgentStatus(int agentId, int status)
    {
        string sql = "UPDATE agents SET status = @status WHERE id = @agentId";
        int rowsAffected = MySqlHelper.Execute(sql, new { status, agentId });
        return rowsAffected > 0;
    }

}