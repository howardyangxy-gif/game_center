namespace app.Common
{
    public class AgentRequestBase
    {
        public int agentId { get; set; } = 0;
        public WalletType walletType { get; set; } = WalletType.Single;
    }
}
