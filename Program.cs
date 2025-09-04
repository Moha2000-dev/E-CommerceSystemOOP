using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AutoMapper.QueryableExtensions;
using E_CommerceSystem.Infrastructure.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using E_CommerceSystem.Middleware;
using Serilog;

namespace E_CommerceSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddScoped<IOrderSummaryRepo, OrderSummaryRepo>();
            builder.Services.AddScoped<IOrderSummaryService, OrderSummaryService>();

            // Add services to the container.
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddScoped<E_CommerceSystem.Repositories.IAdminReportRepo, E_CommerceSystem.Repositories.AdminReportRepo>();
            builder.Services.AddScoped<E_CommerceSystem.Services.IAdminReportService, E_CommerceSystem.Services.AdminReportService>();
            builder.Services.AddScoped<IProductRepo, ProductRepo>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderProductsRepo, OrderProductsRepo>();
            builder.Services.AddScoped<IOrderProductsService, OrderProductsService>();
            builder.Services.AddScoped<IOrderRepo, OrderRepo>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReviewRepo, ReviewRepo>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IProductQueryRepo, ProductQueryRepo>();
            builder.Services.AddScoped<IProductQueryService, ProductQueryService>();
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
            builder.Services.AddScoped<ICookieTokenWriter, CookieTokenWriter>();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            // Auth Service
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Also make sure you register these if you use them:
            builder.Services.AddScoped<IAuthRepo, AuthRepo>();
            builder.Services.AddScoped<ICookieTokenWriter, CookieTokenWriter>();


            // JWT Authentication setup
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("access_token", out var jwt))
                            ctx.Token = jwt;
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("AdminOrManager", p => p.RequireRole("Admin", "Manager"));
            });

            // Swagger/OpenAPI configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer <token>')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            builder.Host.UseSerilog();
            // Configure Serilog before adding services
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)   // read from appsettings.json
                    .ReadFrom.Services(services)                     // dependency injection
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //  Configure Serilog from appsettings.json
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles(); // make sure this is present (IMAGES)


            app.MapControllers();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            Log.Information("E_CommerceSystem starting up");

            app.Run();
        }
    }
}
