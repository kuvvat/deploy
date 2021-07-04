using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Api.Configuration;
using Api.Helpers;
using Api.Middlewares;
using Application.Interfaces;
using Application.Services;
using AspNetCoreRateLimit;
using Core;
using Infrastructure.Context;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;

namespace Api
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private const string CORS_POLICY_NAME = "allowAllPolicy";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="webHostEnvironment">environment</param>
        /// <param name="configuration">configuration</param>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services">services</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // reduces the number of requests a client or proxy makes to a web server
            services.AddResponseCaching();

            // IP Rate limiter
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimit"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // configure MVC services for controllers
            services.AddControllers();

            // configure swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API",
                    Version = "v1",
                    Description = "LinkedIn API",
                    Contact = new OpenApiContact
                    {
                        Name = "Wassim AZIRAR",
                        Email = "wassim.azirar@gmail.com",
                        Url = new Uri("https://www.linkedin.com/in/wassimazirar/")
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using Bearer",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "basic",
                    In = ParameterLocation.Header
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // compression
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            // CORS policy
            services.AddCors(options => options.AddPolicy(CORS_POLICY_NAME, builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 10 * 1024 * 1024; // The default value is 64 * 1024 * 1024 (64 MB)
                options.SizeLimit = 18 * 1024 * 1024; // The default value is 100 * 1024 * 1024 (100 MB)
                options.UseCaseSensitivePaths = true; // The default value is false
            });

            // Add memory cache
            services.AddMemoryCache();

            // Enforce HTTPS in production
            // => Require HTTPS for all requests
            // => Redirect all HTTP requests to HTTPS
            if (!_webHostEnvironment.IsDevelopment())
            {
                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });

                services.AddHttpsRedirection(options => options.HttpsPort = 443);
            }

            // Disable reference looping for EF
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            // Configure strongly typed settings objects
            var appSettingsSection = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // Configure jwt for Authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret ?? string.Empty);
            services.AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !_webHostEnvironment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                // It doesn't work anymore
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context => throw new AuthenticationException("Invalid token")
                };
            });

            // HealthCheck
            services.AddHealthChecks();

            // HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Logger
            services.AddTransient<ITechnicalLogger, TechnicalLogger>(provider => new TechnicalLogger(Log.Logger, provider.GetService<IHttpContextAccessor>()));

            // Services
            RegisterServices(services);

            // Repositories
            RegisterRepositories(services);
        }

        /// <summary>
        /// Configure middle-wares
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="environment">environment</param>
        /// <param name="applicationLifetime">Application lifetime</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IHostApplicationLifetime applicationLifetime)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }


            app.UseSerilogRequestLogging();

            // global error handler
            app.UseMiddleware<ErrorLoggingMiddleware>();

            app.UseIpRateLimiting();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(CORS_POLICY_NAME);

            app.UseResponseCaching();

            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                options.RoutePrefix = string.Empty;
            });

            app.UseHealthChecks("/health");

            app.UseResponseCompression();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            // Configure application startup
            applicationLifetime?.ApplicationStarted.Register(() => OnStart(app));
        }

        private static void OnStart(IApplicationBuilder app)
        {
            Log.Logger.Information("Application started");

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ApplicationContext>();
            var pendingMigrations = context.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                context.Database.Migrate();
            }
        }

        /// <summary>
        /// Register services
        /// </summary>
        /// <param name="services">services collection</param>
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
        }

        /// <summary>
        /// Register repositories
        /// </summary>
        /// <param name="services">services collection</param>
        public void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();

            // UoW and Ef
            var connection = _configuration["ConnectionString"];
            services.AddDbContext<ApplicationContext>(op => op.UseNpgsql(connection, prj => prj.MigrationsAssembly("Infrastructure")));
        }
    }
}
