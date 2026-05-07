
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using VendorHub.Filters;
using VendorHub.Helpers;
using VendorHub.Hubs;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.Settings;

namespace VendorHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Understand what this do
            // After building the app or during configuration:
            var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
            ProductHelper.BaseImageUrl = $"{jwtOptions?.IssuerIP}/Images/Products";
            #endregion


            #region SignalR
            builder.Services.AddSignalR();
            #endregion

            // Add services to the container.

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
            builder.Services.AddSwaggerGen();


            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));

            builder.Services.AddDbContext<VendorHubDbContext>(options =>
            {
                options
                .UseSqlServer(builder.Configuration.GetConnectionString("sqlServerCs"));

                if (builder.Environment.IsDevelopment())
                    options.EnableDetailedErrors();
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


            builder.Services
             .AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
             });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });

            //    options.AddPolicy("RestrictedPolicy", policy =>
            //    {
            //        policy
            //            .WithOrigins("https://yourdomain.com")
            //            .WithMethods("GET", "POST", "PUT", "DELETE")
            //            .WithHeaders("Content-Type", "Authorization");
            //    });
            //});
        });
   
            var app = builder.Build();

            #region SignalR
            // ? Map the hub to a route
            app.MapHub<NotificationHub>("/notificationHub");
            // ? Very important: Enable WebSocket support
            app.UseWebSockets();
            #endregion

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

            app.Run();
        }
    }
}
