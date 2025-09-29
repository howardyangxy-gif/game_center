using app.Common;

namespace app.Common
{
    /// <summary>
    /// 共用工具函數類別
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// 將金額轉換為分數（基於匯率）
        /// </summary>
        /// <param name="amount">原始金額</param>
        /// <param name="exchangeRate">匯率（相對於基底貨幣的比率）</param>
        /// <returns>轉換後的分數</returns>
        public static long ConvertAmountToPoints(decimal amount, decimal exchangeRate)
        {
            if (exchangeRate <= 0)
                throw new ArgumentException("匯率必須大於0", nameof(exchangeRate));

            // 金額 × 匯率 = 基底貨幣金額（台幣分）
            decimal result = amount * exchangeRate;
            return (long)Math.Round(result, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 將分數轉換回金額（基於匯率）
        /// </summary>
        /// <param name="points">分數</param>
        /// <param name="exchangeRate">匯率（相對於基底貨幣的比率）</param>
        /// <returns>轉換後的金額</returns>
        public static decimal ConvertPointsToAmount(long points, decimal exchangeRate)
        {
            if (exchangeRate <= 0)
                throw new ArgumentException("匯率必須大於0", nameof(exchangeRate));

            // 分數 ÷ 匯率 = 原始貨幣金額
            return (decimal)points / exchangeRate;
        }

        /// <summary>
        /// 將元轉換為分（台幣專用，100分=1元）
        /// </summary>
        /// <param name="yuan">元</param>
        /// <returns>分</returns>
        public static long ConvertYuanToCents(decimal yuan)
        {
            return (long)Math.Round(yuan * 100, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 將分轉換為元（台幣專用，100分=1元）
        /// </summary>
        /// <param name="cents">分</param>
        /// <returns>元</returns>
        public static decimal ConvertCentsToYuan(long cents)
        {
            return (decimal)cents / 100;
        }

        /// <summary>
        /// 根據貨幣類型和匯率表轉換金額為基底分數
        /// </summary>
        /// <param name="amount">原始金額</param>
        /// <param name="currencyType">貨幣類型</param>
        /// <returns>轉換後的基底分數（台幣分）</returns>
        public static long ConvertCurrencyToBasePoints(decimal amount, CurrencyType currencyType)
        {
            decimal exchangeRate = GetExchangeRate(currencyType);
            return ConvertAmountToPoints(amount, exchangeRate);
        }

        /// <summary>
        /// 根據貨幣類型和匯率表將基底分數轉換為金額
        /// </summary>
        /// <param name="points">基底分數（台幣分）</param>
        /// <param name="currencyType">貨幣類型</param>
        /// <returns>轉換後的金額</returns>
        public static decimal ConvertBasePointsToCurrency(long points, CurrencyType currencyType)
        {
            decimal exchangeRate = GetExchangeRate(currencyType);
            return ConvertPointsToAmount(points, exchangeRate);
        }

        /// <summary>
        /// 取得指定貨幣的匯率（基於你提供的匯率表）
        /// </summary>
        /// <param name="currencyType">貨幣類型</param>
        /// <returns>匯率</returns>
        private static decimal GetExchangeRate(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.CNY => 500.00000m,   // 人民幣
                CurrencyType.USDT => 3000.00000m, // 泰達幣
                CurrencyType.TWD => 100.00000m,   // 新台幣（基底）
                CurrencyType.JPY => 25.00000m,    // 日幣
                CurrencyType.EUR => 3500.00000m,  // 歐元
                CurrencyType.THB => 100.00000m,   // 泰銖
                CurrencyType.INR => 25.00000m,    // 印度盧比
                CurrencyType.VNDK => 150.00000m,  // 越南盾(K)
                CurrencyType.MYR => 500.00000m,   // 馬幣
                CurrencyType.KRWK => 2500.00000m, // 韓元(K)
                _ => throw new ArgumentException($"不支援的貨幣類型: {currencyType}", nameof(currencyType))
            };
        }

        /// <summary>
        /// 格式化金額顯示（根據貨幣類型添加適當的小數位數）
        /// </summary>
        /// <param name="amount">金額</param>
        /// <param name="currencyType">貨幣類型</param>
        /// <returns>格式化後的金額字串</returns>
        public static string FormatCurrency(decimal amount, CurrencyType currencyType)
        {
            var currencySymbol = GetCurrencySymbol(currencyType);
            var decimalPlaces = GetDecimalPlaces(currencyType);
            
            return $"{currencySymbol}{amount.ToString($"F{decimalPlaces}")}";
        }

        /// <summary>
        /// 取得貨幣符號
        /// </summary>
        /// <param name="currencyType">貨幣類型</param>
        /// <returns>貨幣符號</returns>
        private static string GetCurrencySymbol(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.CNY => "¥",      // 人民幣
                CurrencyType.USDT => "₮",     // 泰達幣
                CurrencyType.TWD => "NT$",    // 新台幣
                CurrencyType.JPY => "¥",      // 日幣
                CurrencyType.EUR => "€",      // 歐元
                CurrencyType.THB => "฿",      // 泰銖
                CurrencyType.INR => "₹",      // 印度盧比
                CurrencyType.VNDK => "₫",     // 越南盾
                CurrencyType.MYR => "RM",     // 馬幣
                CurrencyType.KRWK => "₩",     // 韓元
                _ => "$"
            };
        }

        /// <summary>
        /// 取得貨幣的小數位數
        /// </summary>
        /// <param name="currencyType">貨幣類型</param>
        /// <returns>小數位數</returns>
        private static int GetDecimalPlaces(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.JPY => 0,        // 日幣通常不用小數
                CurrencyType.KRWK => 0,       // 韓元通常不用小數
                CurrencyType.VNDK => 0,       // 越南盾通常不用小數
                _ => 2                        // 其他貨幣使用2位小數
            };
        }

        /// <summary>
        /// 驗證金額是否為正數
        /// </summary>
        /// <param name="amount">金額</param>
        /// <param name="paramName">參數名稱（用於例外訊息）</param>
        /// <returns>驗證通過返回 true</returns>
        /// <exception cref="ArgumentException">金額小於等於0時拋出例外</exception>
        public static bool ValidatePositiveAmount(decimal amount, string paramName = "amount")
        {
            if (amount <= 0)
                throw new ArgumentException($"金額必須大於0，目前值: {amount}", paramName);
            
            return true;
        }

        /// <summary>
        /// 安全的除法運算（避免除以零）
        /// </summary>
        /// <param name="dividend">被除數</param>
        /// <param name="divisor">除數</param>
        /// <param name="defaultValue">除數為0時的預設值</param>
        /// <returns>除法結果</returns>
        public static decimal SafeDivision(decimal dividend, decimal divisor, decimal defaultValue = 0)
        {
            return divisor == 0 ? defaultValue : dividend / divisor;
        }

        /// <summary>
        /// 取得錯誤代碼對應的錯誤訊息
        /// </summary>
        /// <param name="errorCode">錯誤代碼</param>
        /// <returns>錯誤訊息</returns>
        public static string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                0 => "操作成功",
                
                // 認證與授權錯誤
                100 => "認證失敗",
                101 => "無效的令牌",
                102 => "令牌過期",
                103 => "存取被拒絕",
                104 => "簽名驗證失敗",
                105 => "Nonce 驗證失敗",
                106 => "IP 不在白名單",
                
                // 代理商相關錯誤
                200 => "代理商不存在",
                201 => "代理商狀態無效",
                202 => "代理商餘額不足",
                203 => "代理商錢包被鎖定",
                
                // 玩家相關錯誤
                300 => "玩家不存在",
                301 => "玩家名稱無效",
                302 => "玩家已存在",
                303 => "玩家餘額不足",
                304 => "玩家錢包被鎖定",
                
                // SQL Stored Procedure 錯誤
                1000 => "內部錯誤",
                1001 => "代理商不存在",
                1002 => "代理商餘額不足",
                1003 => "玩家錢包不存在",
                1004 => "玩家餘額不足",
                
                // 系統層級技術錯誤
                9001 => "SP執行成功但無返回結果",
                9002 => "資料庫連線錯誤",
                9003 => "資料庫執行錯誤",
                
                _ => $"未知錯誤代碼: {errorCode}"
            };
        }

        /// <summary>
        /// 取得錯誤代碼對應的錯誤訊息（使用 ErrorCode 枚舉）
        /// </summary>
        /// <param name="errorCode">錯誤代碼枚舉</param>
        /// <returns>錯誤訊息</returns>
        public static string GetErrorMessage(ErrorCode errorCode)
        {
            return GetErrorMessage((int)errorCode);
        }

        /// <summary>
        /// 檢查是否為成功的錯誤代碼
        /// </summary>
        /// <param name="errorCode">錯誤代碼</param>
        /// <returns>是否成功</returns>
        public static bool IsSuccessCode(int errorCode)
        {
            return errorCode == (int)ErrorCode.Success;
        }
    }
}