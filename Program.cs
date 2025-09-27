using app.Services;
using app.Infrastructure;
using app.Servers.Agent.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/system-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Properties.ContainsKey("ServiceName") && 
                                      evt.Properties["ServiceName"].ToString().Contains("Agent"))
        .WriteTo.File(
            path: "logs/{Date}/agent.log",
            rollingInterval: RollingInterval.Day,
            formatProvider: null,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Properties.ContainsKey("ServiceName") && 
                                      evt.Properties["ServiceName"].ToString().Contains("Game"))
        .WriteTo.File(
            path: "logs/{Date}/game.log",
            rollingInterval: RollingInterval.Day,
            formatProvider: null,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Properties.ContainsKey("ServiceName") && 
                                      evt.Properties["ServiceName"].ToString().Contains("Player"))
        .WriteTo.File(
            path: "logs/{Date}/player.log",
            rollingInterval: RollingInterval.Day,
            formatProvider: null,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog
builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 註冊dbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// DI 註冊 Services
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<PlayerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 模組化註冊 Endpoints
app.MapAgentEndpoints();
app.MapGameEndpoints();
app.MapSessionEndpoints();
app.MapPlayerEndpoints();

app.MapGet("/", () => "Center API is running!");

try
{
    Log.Information("Starting Game Center API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Game Center API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

