using System.Text;
using ChimeBackend.Application.Services;
using ChimeBackend.Domain.Repositories;
using ChimeBackend.Infrastructure.Data;
using ChimeBackend.Infrastructure.Extensions;
using ChimeBackend.Infrastructure.Repositories;
using ChimeBackend.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings.json (DDD-style)
builder.Services.AddSerilogLogging(builder.Configuration);
builder.Host.UseSerilog();

try
{
    // Add services
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure EF Core with multiple databases
    var chimeDbConnection = builder.Configuration.GetConnectionString("ChimeDb")
        ?? throw new InvalidOperationException("ChimeDb connection string is not set");
    var foodLibraryDbConnection = builder.Configuration.GetConnectionString("FoodLibraryDb")
        ?? throw new InvalidOperationException("FoodLibraryDb connection string is not set");

    builder.Services.AddDbContext<ChimeDbContext>(options =>
        options.UseSqlServer(chimeDbConnection));

    builder.Services.AddDbContext<FoodLibraryDbContext>(options =>
        options.UseSqlServer(foodLibraryDbConnection));

    // Configure JWT Authentication
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
    builder.Services.AddSingleton(jwtSettings);

    // Configure WeChat Settings
    var weChatSettings = new WeChatSettings();
    builder.Configuration.GetSection(WeChatSettings.SectionName).Bind(weChatSettings);
    builder.Services.AddSingleton(weChatSettings);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // Register application services
    builder.Services.AddSingleton<ITokenService, TokenService>();
    builder.Services.AddSingleton<IVerificationCodeService, InMemoryVerificationCodeService>();
    builder.Services.AddHttpClient("WeChat");
    builder.Services.AddScoped<IWxService>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("WeChat");
        var logger = sp.GetRequiredService<ILogService>();
        var weChatSettings = sp.GetRequiredService<WeChatSettings>();
        return new WxService(httpClient, logger, weChatSettings);
    });

    // Register repositories
    builder.Services.AddScoped<IFoodRecordRepository, FoodRecordRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
    builder.Services.AddScoped<IFoodRepository, FoodRepository>();
    builder.Services.AddScoped<IFoodCategoryRepository, FoodCategoryRepository>();

    // Register application services
    builder.Services.AddScoped<FoodRecordAppService>();
    builder.Services.AddScoped<UserAppService>();
    builder.Services.AddScoped<AuthAppService>();
    builder.Services.AddScoped<FoodAppService>();

    var app = builder.Build();

    // Test database connection
    using (var scope = app.Services.CreateScope())
    {
        var chimeDbContext = scope.ServiceProvider.GetRequiredService<ChimeDbContext>();
        try
        {
            await chimeDbContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "✗ ChimeDb connection failed");
        }

        var foodDbContext = scope.ServiceProvider.GetRequiredService<FoodLibraryDbContext>();
        try
        {
            await foodDbContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "✗ FoodLibraryDb connection failed");
        }
    }

    if (builder.Configuration.GetValue<bool>("Swagger:Enabled", false))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
