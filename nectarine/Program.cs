using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NectarineAPI.Configurations;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineAPI.Services.Auth;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using NectarineData.Products;
using Newtonsoft.Json;
using Serilog;
using Stripe;
using TokenOptions = NectarineAPI.Configurations.TokenOptions;
using TokenService = NectarineAPI.Services.TokenService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(builder.Configuration["ConnectionStrings:AppConfig"]);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    Host.CreateDefaultBuilder().ConfigureAppConfiguration(b =>
        b.AddAzureAppConfiguration(builder.Configuration["ConnectionStrings:AppConfig"]))
        .UseSerilog();

    await ConfigureServices(builder.Services);
    Configure();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

async Task ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient();

    services.AddHttpContextAccessor();

    services.AddDbContext<NectarineDbContext>(options =>
        // options.UseSqlServer("Server=tcp:nectarine.database.windows.net,1433;Initial Catalog=uks-nectarine-sqldb;Persist Security Info=False;User ID=stefannovak96@gmail.com@nectarine;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));
        options.UseSqlServer("Server=tcp:127.0.0.1,1433;User Id=sa;Password=<Password123>;Database=nectarine;MultipleActiveResultSets=true;TrustServerCertificate=true"));

    services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<NectarineDbContext>();

    ConfigureJWTAuthentication(services);
    ConfigureApplicationServices(services);
    ConfigureHangfire(services);

    services.AddControllers();

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nectarine", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "Specify the authorization token.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
        });

        OpenApiSecurityScheme securityScheme = new ()
        {
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme,
            },
            In = ParameterLocation.Header,
            Name = "Bearer",
            Scheme = "Bearer",
        };

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() },
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

    await new ProductGenerator().GenerateAndSeedProducts("Server=tcp:127.0.0.1,1433;User Id=sa;Password=<Password123>;Database=nectarine;MultipleActiveResultSets=true;TrustServerCertificate=true");
}

void Configure()
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nectarine v1"));
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHangfireDashboard();
    app.MapControllers();
    app.MapHangfireDashboard();

    app.Run();
}

void ConfigureApplicationServices(IServiceCollection services)
{
    StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:Secret").Value;
    services.Configure<TwilioOptions>(builder.Configuration.GetSection("Twilio"));
    services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));

    services.AddTransient<IPaymentService, PaymentService>();
    services.AddTransient<IUserCustomerService, UserCustomerService>();
    services.AddTransient<ITokenService, TokenService>();
    services.AddTransient<IPhoneService, TwilioService>();
    services.AddTransient<IEmailService, SendGridEmailService>();

    services.AddTransient<IExternalAuthService<GoogleUser>, GoogleAuthService<GoogleUser>>();
    services.AddTransient<IExternalAuthService<MicrosoftUser>, MicrosoftAuthService<MicrosoftUser>>();
    services.AddTransient<IExternalAuthService<FacebookUser>, FacebookAuthService<FacebookUser>>();
}

void ConfigureJWTAuthentication(IServiceCollection services)
{
    services.Configure<TokenOptions>(builder.Configuration.GetSection("JWT"));
    var tokenConfig = builder.Configuration.GetSection("JWT").Get<TokenOptions>() ?? new TokenOptions();

    services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = true;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = tokenConfig.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenConfig.Secret)),
                ValidAudience = tokenConfig.Audience,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
        });
}

void ConfigureHangfire(IServiceCollection services)
{
    services.AddHangfire(options => options
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseColouredConsoleLogProvider()
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSerializerSettings(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
        .UseSqlServerStorage(
            // "Server=tcp:nectarine.database.windows.net,1433;Initial Catalog=uks-nectarine-sqldb;Persist Security Info=False;User ID=stefannovak96@gmail.com@nectarine;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
            "Server=tcp:127.0.0.1,1433;User Id=sa;Password=<Password123>;Database=nectarine;MultipleActiveResultSets=true;TrustServerCertificate=true",
            new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(5),
            UseRecommendedIsolationLevel = true,
            UsePageLocksOnDequeue = true,
            DisableGlobalLocks = true,
        }));

    services.AddHangfireServer();
}