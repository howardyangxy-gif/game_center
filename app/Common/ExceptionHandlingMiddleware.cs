using System.Text.Json;
using app.Common.Logging;

namespace app.Common
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 啟用 Request Body 緩衝，讓我們能重複讀取
            context.Request.EnableBuffering();
            
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400; // Bad Request

            // 記錄請求資訊
            string requestBody = "";
            try
            {
                // 重置 Request Body 位置以便讀取
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                
                using var reader = new StreamReader(context.Request.Body);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
            catch
            {
                requestBody = "無法讀取請求內容";
            }

            // 記錄詳細資訊
            var logData = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                Headers = context.Request.Headers.ToDictionary(h => h.Key, h => string.Join(";", h.Value.ToArray())),
                RequestBody = requestBody,
                Exception = ex.Message,
                StackTrace = ex.StackTrace
            };

            LogService.System.LogError("Request failed due to exception", ex, logData);

            // 統一回傳格式
            var response = new
            {
                code = (int)ErrorCode.InvalidParameter,
                errorMsg = "請求參數格式錯誤"
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}