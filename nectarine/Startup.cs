using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using Stripe;
using TokenOptions = NectarineAPI.Configurations.TokenOptions;
using TokenService = NectarineAPI.Services.TokenService;

namespace NectarineAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.AddDbContext<NectarineDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<NectarineDbContext>();

            ConfigureJWTAuthentication(services);
            ConfigureApplicationServices(services);

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nectarine v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureApplicationServices(IServiceCollection services)
        {
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe:Secret").Value;
            services.Configure<TwilioOptions>(Configuration.GetSection("Twilio"));
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGrid"));

            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IUserCustomerService, UserCustomerService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IPhoneService, TwilioService>();
            services.AddTransient<IEmailService, SendGridEmailService>();

            services.AddTransient<IExternalAuthService<GoogleUser>, GoogleAuthService<GoogleUser>>();
            services.AddTransient<IExternalAuthService<MicrosoftUser>, MicrosoftAuthService<MicrosoftUser>>();
            services.AddTransient<IExternalAuthService<FacebookUser>, FacebookAuthService<FacebookUser>>();
        }

        private void ConfigureJWTAuthentication(IServiceCollection services)
        {
            services.Configure<TokenOptions>(Configuration.GetSection("JWT"));
            var tokenConfig = Configuration.GetSection("JWT").Get<TokenOptions>();

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
    }
}
