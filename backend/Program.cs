using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;
using VendorHub.Events;
using VendorHub.Exceptions;
using VendorHub.Extensions;
using VendorHub.Filters;
using VendorHub.Helpers;
using VendorHub.Hubs;
using VendorHub.Middleware;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Services.Caching;
using VendorHub.Services.Storage;
using VendorHub.Settings;

namespace VendorHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig.ReadFrom.Configuration(context.Configuration);
            });

            var jwtOptions = builder.Configuration.GetSection("JWT").Get<JwtOptions>();

            var sqlConnectionString = builder.Configuration.GetConnectionString("sqlServerCs");

            var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");

            builder.Services.AddProblemDetails(config =>
            {
                config.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddSignalR();
            builder.Services.AddHealthChecks()
                .AddCheck("Database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database operational"));

            builder.Services.AddMemoryCache();
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "VendorHub_";
                });
            }

            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services
                .AddControllers(options =>
                {
                    options.Filters.Add<ValidateModelStateFilter>();
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "VendorHub API",
                        Version = "v1",
                        Description = "VendorHub E-Commerce REST API with JWT Bearer Authentication"
                    });

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter 'Bearer' [space] and then your valid JWT token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
                    };

                    options.AddSecurityDefinition("Bearer", securityScheme);

                    options.AddSecurityRequirement((doc) => new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer"),
                            new List<string>()
                        }
                    });
                });

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));
            builder.Services.AddDbContext<VendorHubDbContext>(options =>
            {
                options.UseSqlServer(sqlConnectionString);

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            builder.Services
                .AddIdentity<User, IdentityRole<int>>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<VendorHubDbContext>()
                .AddDefaultTokenProviders();


            builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));

            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IVendorService, VendorService>();
            builder.Services.AddScoped<IFavoriteService, FavoriteService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IStatisticsService, StatisticsService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IVendorService, VendorService>();

            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IImageValidator, ImageValidator>();

            builder.Services.AddSingleton(typeof(IEventQueue<>), typeof(EventQueue<>));
            builder.Services.AddHostedService<EventConsumerBackgroundService<OrderPlacedEvent>>();

            builder.Services.AddScoped<ICustomEventHandler<OrderPlacedEvent>, SignalrOrderPlacedHandler>();
            builder.Services.AddScoped<ICustomEventHandler<OrderPlacedEvent>, DbOrderPlacedHandler>();
            builder.Services.AddScoped<EventPublisher>();

            var secretKey = jwtOptions?.SecritKey;
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                secretKey = builder.Configuration["JWT_SECRET_KEY"] ?? throw new InvalidOperationException(
            "JWT Secret Key is missing from configuration! Set 'JWT:SecritKey' in appsettings or 'JWT_SECRET_KEY' environment variable."); ;
            }

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions?.IssuerIP ?? builder.Configuration["JWT_ISSUER"],
                        ValidateAudience = true,
                        ValidAudience = jwtOptions?.AudienceIP ?? builder.Configuration["JWT_AUDIENCE"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
            Context.Configure(httpContextAccessor);

            app.UseExceptionHandler();

            app.UseWebSockets();
            app.UseStaticFiles();

            app.UseMiddleware<RequestLogContextMiddleware>();
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                };
            });

            app.UseCors("MyPolicy");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VendorHub API v1");
                c.RoutePrefix = "swagger";
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/swagger");
                    return;
                }
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHealthChecks("/health");

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                var dbContext = services.GetRequiredService<VendorHubDbContext>();
                if (dbContext.Database.IsSqlServer())
                {
                    await dbContext.Database.MigrateAsync();
                }

                await RoleSeeder.SeedAsync(services);
                var accountService = services.GetRequiredService<IAccountService>();
                var adminEmail = builder.Configuration["SeedData:AdminEmail"] ?? "admin@gmail.com";
                var adminPassword = builder.Configuration["SeedData:AdminPassword"] ?? "P@ssw0rd123!";

                await accountService.CreateFirstAdminAsync(
                    firstName: "Super",
                    secondName: "Admin",
                    email: adminEmail,
                    password: adminPassword,
                    phone: "01234567891"
                );

                var permissionService = services.GetRequiredService<IPermissionService>();
                var vendorService = services.GetRequiredService<IVendorService>();
                var vendors = await vendorService.GetAllVendorsAsync();
                if (vendors?.Data?.Items != null)
                {
                    foreach (var vendor in vendors.Data.Items)
                    {
                        await permissionService.EnablePermissionForVendorAsync(vendor.Id, PermissionType.CanViewProducts);
                        await permissionService.EnablePermissionForVendorAsync(vendor.Id, PermissionType.CanViewOrders);
                        await permissionService.EnablePermissionForVendorAsync(vendor.Id, PermissionType.CanUpdateOrderStatus);
                    }
                }

            }
            app.Run();
        }
    }
}
