using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VendorHub.Filters;
using VendorHub.GraphQL;
using VendorHub.Helpers;
using VendorHub.Hubs;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Services.Caching;
using VendorHub.Settings;

namespace VendorHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
            ProductHelper.BaseImageUrl = $"{jwtOptions?.IssuerIP}/Images/Products";

            builder.Services.AddSignalR();

            builder.Services.AddMemoryCache();
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "VendorHub_";
            });

            builder.Services.AddScoped<ICacheService, CacheService>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddFiltering()
            .AddSorting();

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
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddSwaggerGen();
            }


            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));

            builder.Services.AddDbContext<VendorHubDbContext>(options =>
            {
                options
                .UseSqlServer(builder.Configuration.GetConnectionString("sqlServerCs"));

                if (builder.Environment.IsDevelopment())
                    options.EnableDetailedErrors();
                //else options.EnableSensitiveDataLogging = false;
            });


            builder.Services
                .AddIdentity<User, IdentityRole<int>>
                (options => {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<VendorHubDbContext>();
                //.AddDefaultTokenProviders();


            builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));

            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IFavoriteService, FavoriteService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IStatisticsService, StatisticsService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IVendorService, VendorService>();

            builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:IssuerIP"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:AudienceIP"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecritKey"] ?? "")),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"]; // SignalR ???? ?????? ???
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


            //builder.Services
            // .AddAuthentication(options =>
            // {
            //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            // })
            // .AddJwtBearer(options => 
            // {
            //     options.SaveToken = true;
            //     options.RequireHttpsMetadata = false;
            //     options.TokenValidationParameters = new TokenValidationParameters()
            //     {
            //         ValidateIssuer = true,
            //         ValidIssuer = builder.Configuration["JWT:IssuerIP"],

            //         ValidateAudience = true,
            //         ValidAudience = builder.Configuration["JWT:AudienceIP"],

            //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecritKey"] ?? "")),

            //         ValidateLifetime = true,
            //         ClockSkew = TimeSpan.Zero
            //     };
            // });


            //    builder.Services.AddCors(options =>
            //    {
            //        options.AddPolicy("MyPolicy", policy =>
            //        {
            //            policy
            //            .AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader();
            //        });

            //    //    options.AddPolicy("RestrictedPolicy", policy =>
            //    //    {
            //    //        policy
            //    //            .WithOrigins("https://yourdomain.com")
            //    //            .WithMethods("GET", "POST", "PUT", "DELETE")
            //    //            .WithHeaders("Content-Type", "Authorization");
            //    //    });
            //    //});
            //});

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")  // Your Vite dev server
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();                   // Required for SignalR
                });
            });

            var app = builder.Build();

            //app.MapHealthChecks("/health");
            app.UseWebSockets();
            app.MapHub<NotificationHub>("/notificationHub");

            // middleWare
            if (app.Environment.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/")
                    {
                        context.Response.Redirect("/swagger");
                        return;
                    }
                    await next();
                });

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseCors("MyPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<VendorHubDbContext>();
                await dbContext.Database.MigrateAsync();
                await PermissionSeeder.SeedAsync(services);
                await RoleSeeder.SeedAsync(services);
                var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                await accountService.CreateFirstAdminAsync(
                    firstName: "Super",
                    secondName: "Admin",
                    email: "admin@gmail.com",
                    password: "P@ssw0rd",
                    phone: "01234567891"
                );
            }

            using (var scope = app.Services.CreateScope())
            {
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                var vendorService = scope.ServiceProvider.GetRequiredService<IVendorService>();
                var vendors = await vendorService.GetAllVendorsAsync();
                foreach (var vendor in vendors.Data)
                {
                    await permissionService.EnablePermissionForVendorAsync(vendor.Id, PermissionType.CanViewProducts);
                    await permissionService.EnablePermissionForVendorAsync(vendor.Id, PermissionType.CanViewOrders);
                    await permissionService.EnablePermissionForVendorAsync(vendor.Id, PermissionType.CanUpdateOrderStatus);
                    // Add others as needed
                }
            }

            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();
        }
    }
}
