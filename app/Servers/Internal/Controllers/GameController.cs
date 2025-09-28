using app.Services;
using app.Common.Logging;

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
            var (isValid, userId, errorMsg) = service.CheckUserToken(verifyRequest);
            if (isValid)
                return Results.Ok(new { success = true, userId });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });

        //遊戲端使用, 取得玩家餘額
        app.MapPost("/api/game/getBalance", (BalanceRequest balanceRequest, GameService service) =>
        {
            var (success, balance, errorMsg) = service.GetPlayerBalance(balanceRequest);

            if (success)
                return Results.Ok(new { success = true, balance });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });
        
        //遊戲端使用, 玩家下注時, 遊戲端同時打下注跟結算
        app.MapPost("/api/game/betAndSettle", (BetRequest betRequest, GameService service) =>
        {

            var (success, balance, errorMsg) = service.PlayerBetAndSettle(betRequest);

            if (success)
                return Results.Ok(new { success = true, balance });
            else
                return Results.BadRequest(new { success = false, message = errorMsg });
        });
    }

}
