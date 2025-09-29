using app.Services;
using app.Common.Logging;
using app.Servers.Internal.Models;

public static class GameController
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        // 暫時測試token使用
        app.MapGet("/api/game/createTestToken", (GameService service) =>
        {
            var token = service.CreateTestToken();
            return Results.Ok(new { success = true, token });
        });

        //暫時測試用, 會傳入TOKEN, 回傳驗證結果
        app.MapPost("/api/game/checkToken", (CheckTokenRequest tokenRequest, GameService service) =>
        {
            var (isValid, userId, errorMsg) = service.CheckTestToken(tokenRequest.token);
            if (isValid)
                return Results.Ok(new { success = true, userId });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });

        //遊戲端使用, 玩家驗證
        app.MapPost("/api/game/checkUserToken", (CheckTokenRequest verifyRequest, GameService service) =>
        {
            var (errorCode, data, errorMsg) = service.CheckUserToken(verifyRequest);
            if (errorCode == 0)
                return Results.Ok(new
                {
                    code = errorCode,
                    data,
                    errorMsg
                });
            else
                return Results.BadRequest(new
                {
                    code = errorCode,
                    errorMsg
                });
        });

        //遊戲端使用, 取得玩家餘額
        app.MapPost("/api/game/getBalance", (BalanceRequest balanceRequest, GameService service) =>
        {
            var (errorCode, data, errorMsg) = service.GetPlayerBalance(balanceRequest);

            if (errorCode == 0)
                return Results.Ok(new { 
                    code = errorCode,
                    data,
                    errorMsg
                });
            else
                return Results.BadRequest(new { 
                    code = errorCode,
                    errorMsg
                });
        });
        
        //遊戲端使用, 玩家下注時, 遊戲端同時打下注跟結算
        app.MapPost("/api/game/betAndSettle", (BetRequest betRequest, GameService service) =>
        {

            var (errorCode, data, errorMsg) = service.PlayerBetAndSettle(betRequest);

            if (errorCode == 0)
                return Results.Ok(new { 
                    code = errorCode,
                    data,
                    errorMsg
                });
            else
                return Results.BadRequest(new { 
                    code = errorCode,
                    errorMsg
                });
        });
    }

}
