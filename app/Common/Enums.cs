namespace app.Common
{
    /// <summary>
    /// 貨幣類型枚舉
    /// </summary>
    public enum CurrencyType
    {
        TWD = 1,    // 新台幣
        USDT = 2,   // 泰達幣
        CNY = 3,    // 人民幣
        JPY = 4,    // 日幣
        EUR = 5,    // 歐元
        THB = 6,    // 泰銖
        INR = 7,    // 印度盧比
        VNDK = 8,   // 越南盾(K)
        MYR = 9,    // 馬幣
        KRWK = 10,   // 韓元(K)
        PHP = 11,    // 菲律賓比索
    }

    /// <summary>
    /// 錢包類型
    /// </summary>
    public enum WalletType
    {
        Transfer = 0, // 轉帳錢包
        Single = 1    // 單一錢包
    }

    /// <summary>
    /// 語言類型
    /// </summary>
    public enum LanguageType
    {
        EN = 1,     // 英文
        ZH_CN = 2,  // 簡體中文
        ZH_TW = 3,  // 繁體中文
        TH = 4,     // 泰文
        VI = 5,     // 越南文
        KO = 6,     // 韓文
        JA = 7,     // 日文
        ID = 8,     // 印尼文
        MS = 9,     // 馬來文
        HI = 10     // 印地文
    }

    /// <summary>
    /// 錢包帳變操作
    /// </summary>
    public enum WalletAction
    {
        PlayerDeposit = 1,        // 玩家钱包上分
        PlayerDepositRollback = 2, // 玩家钱包上分回滾
        PlayerWithdraw = 3,       // 玩家钱包下分
        PlayerWithdrawRollback = 4 // 玩家钱包下分回滾
    }
    
    /// <summary>
    /// 錢包帳變類型
    /// </summary>
    public enum WalletTransactionType
    {
        // 轉帳錢包類型 (1-10)
        TransferDeposit = 1,           // 轉帳錢包上分
        TransferDepositRollback = 2,   // 轉帳錢包上分回滾
        TransferWithdraw = 3,          // 轉帳錢包下分
        TransferWithdrawRollback = 4,  // 轉帳錢包下分回滾

        // 遊戲類型 (5-10)
        GameBet = 5,                   // 遊戲下注
        GameWin = 6,                   // 遊戲派獎
        GameCancelBet = 7,             // 遊戲取消下注

        // 單一錢包類型 (11-20)
        SingleGameBet = 11,                // 單一錢包遊戲下注
        SingleGameBetRollback = 12,        // 單一錢包遊戲下注回滾
        SingleGameWin = 13,                // 單一錢包遊戲派獎
        SingleGameWinRollback = 14,        // 單一錢包遊戲派獎回滾
        SingleGameCancelBet = 15,          // 單一錢包遊戲取消下注
        SingleGameCancelBetRollback = 16   // 單一錢包遊戲取消下注回滾
    }

    /// <summary>
    /// 轉帳錢包錯誤代碼
    /// </summary>
    public enum ErrorCode
    {
        // 成功 (0)
        Success = 0,                    // 操作成功

        // SQL Stored Procedure 錯誤代碼 (1000-1099)
        SqlInternalError = 1000,        // 內部錯誤
        SqlAgentNotExists = 1001,       // 代理不存在
        SqlAgentInsufficientBalance = 1002, // 代理餘額不足
        SqlPlayerWalletNotExists = 1003,    // 玩家錢包不存在
        SqlPlayerInsufficientBalance = 1004, // 玩家餘額不足
        
        // 一般應用程式錯誤 (1-99)
        GeneralError = 1,               // 一般錯誤
        InvalidParameter = 2,           // 參數錯誤
        DatabaseError = 3,              // 資料庫錯誤
        NetworkError = 4,               // 網路錯誤
        SystemError = 5,                // 系統錯誤
        ConfigurationError = 6,         // 配置錯誤

        // 認證與授權錯誤 (100-199)
        AuthenticationFailed = 100,     // 認證失敗
        InvalidToken = 101,             // 無效的令牌
        TokenExpired = 102,             // 令牌過期
        AccessDenied = 103,             // 存取被拒絕
        InvalidSignature = 104,         // 簽名驗證失敗
        InvalidNonce = 105,             // Nonce 驗證失敗
        IpNotAllowed = 106,             // IP 不在白名單

        // 代理商相關錯誤 (200-299)
        AgentNotFound = 200,            // 代理商不存在
        AgentStatusInvalid = 201,       // 代理商狀態無效
        AgentBalanceInsufficient = 202, // 代理商餘額不足
        AgentWalletLocked = 203,        // 代理商錢包被鎖定

        // 玩家相關錯誤 (300-399)
        PlayerNotFound = 300,           // 玩家不存在
        PlayerNameInvalid = 301,        // 玩家名稱無效
        PlayerAlreadyExists = 302,      // 玩家已存在
        PlayerBalanceInsufficient = 303, // 玩家餘額不足
        PlayerWalletLocked = 304,       // 玩家錢包被鎖定

        // 系統層級技術錯誤 (9000-9999)
        StoredProcedureNoResult = 9001, // SP 執行成功但無返回結果
        DatabaseConnectionError = 9002, // 資料庫連線錯誤
        DatabaseExecutionError = 9003,  // 資料庫執行錯誤
        
    }
}