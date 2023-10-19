using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
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
using NectarineAPI.Extensions;
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

var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    Host.CreateDefaultBuilder().UseSerilog();
    ConfigureServices(builder.Services);
    await Configure();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient();

    services.AddHttpContextAccessor();

    services.AddDbContext<NectarineDbContext>(options => options.UseNpgsql(sqlConnectionString));

    services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
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
            Type = SecuritySchemeType.Http,
        });

        c.OperationFilter<SecurityRequirementsOperationFilter>();

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
}

async Task Configure()
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

    await GenerateAndSeedProducts();

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
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(sqlConnectionString)));

    services.AddHangfireServer();
}

async Task GenerateAndSeedProducts()
{
    var optionBuilder = new DbContextOptionsBuilder<NectarineDbContext>();
    optionBuilder.UseNpgsql(sqlConnectionString);
    var context = new NectarineDbContext(optionBuilder.Options);

    // Maybe cache this so that a Context doesnt have the be created on startup every time.
    if (context.Products.Any())
    {
        return;
    }

    await context.Database.EnsureCreatedAsync();

    var products = new ProductGenerator().GenerateProducts();

    var timeBeforeSaved = DateTime.Now;
    await context.Products.AddRangeAsync(products);
    await context.SaveChangesAsync();
    Log.Information($"products saved in {(DateTime.Now - timeBeforeSaved).Seconds}");
}