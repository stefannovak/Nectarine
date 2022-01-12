using System;
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
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineAPI.Services.Auth;
using nectarineData.DataAccess;
using nectarineData.Models;
using Stripe;
using TokenOptions = nectarineAPI.Configurations.TokenOptions;
using TokenService = nectarineAPI.Services.TokenService;

namespace nectarineAPI
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
            services.AddDbContext<NectarineDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.AddControllers();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            ConfigureJWTAuthentication(services);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "nectarine", Version = "v1" });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Specify the authorization token.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                OpenApiSecurityScheme securityScheme = new()
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
            });
            
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe:Secret").Value;

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<NectarineDbContext>();
            
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IUserCustomerService, UserCustomerService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddTransient<IExternalAuthService<GoogleUser>, GoogleAuthService<GoogleUser>>();
            services.AddTransient<IExternalAuthService<MicrosoftUser>, MicrosoftAuthService<MicrosoftUser>>();
            services.AddTransient<IExternalAuthService<FacebookUser>, FacebookAuthService<FacebookUser>>();
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "nectarine v1"));
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
