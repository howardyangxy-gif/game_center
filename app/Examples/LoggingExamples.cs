using app.Common.Logging;

namespace app.Examples
{
    public class LoggingExamples
    {
        public static void DemonstrateLogging()
        {
            // Agent 服務日誌範例
            LogService.Agent.LogInfo("Player login successful", new { 
                PlayerId = 123, 
                PlayerName = "test_player",
                LoginTime = DateTime.UtcNow 
            });

            LogService.Agent.LogWarning("Player balance insufficient", new { 
                PlayerId = 123, 
                CurrentBalance = 50.00m,
                RequiredAmount = 100.00m 
            });

            LogService.Agent.LogError("Database connection failed", new Exception("Connection timeout"), new { 
                Server = "localhost",
                Database = "game_center" 
            });

            // Game 服務日誌範例
            LogService.Game.LogInfo("Game round started", new { 
                GameId = "poker_001",
                TableId = "table_5",
                Players = new[] { "player1", "player2" }
            });

            LogService.Game.LogDebug("Player bet placed", new { 
                PlayerId = 123,
                BetAmount = 25.00m,
                BetType = "call"
            });

            // Player 服務日誌範例
            LogService.Player.LogInfo("Player profile updated", new { 
                PlayerId = 789,
                UpdatedFields = new[] { "email", "phone" }
            });

            // System 服務日誌範例
            LogService.System.LogInfo("Application started", new { 
                Version = "1.0.0",
                Environment = "Development",
                StartTime = DateTime.UtcNow
            });
        }
    }
}