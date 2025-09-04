using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using AutoMapper;
using E_CommerceSystem.Infrastructure.Repositories;
using E_CommerceSystem.Middleware;
using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace E_CommerceSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------- Logging (Serilog) ----------
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
            });

            // ---------- Services ----------
            builder.Services.AddControllers();

            // Repos
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IProductRepo, ProductRepo>();
            builder.Services.AddScoped<IOrderProductsRepo, OrderProductsRepo>();
            builder.Services.AddScoped<IOrderRepo, OrderRepo>();
            builder.Services.AddScoped<IReviewRepo, ReviewRepo>();
            builder.Services.AddScoped<IProductQueryRepo, ProductQueryRepo>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IAuthRepo, AuthRepo>();
            builder.Services.AddScoped<IOrderSummaryRepo, OrderSummaryRepo>();
            builder.Services.AddScoped<IAdminReportRepo, AdminReportRepo>();

            // Services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderProductsService, OrderProductsService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IProductQueryService, ProductQueryService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IOrderSummaryService, OrderSummaryService>();
            builder.Services.AddScoped<IAdminReportService, AdminReportService>();

            // Infra / helpers
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
            builder.Services.AddScoped<ICookieTokenWriter, CookieTokenWriter>(); // register once

            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // AutoMapper — disable method mapping to avoid LINQ Max<T> constraint crash
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.ShouldMapMethod = _ => false;
            }, typeof(Program).Assembly);

            // ---------- AuthN / AuthZ ----------
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // NOTE: make sure your appsettings uses ONE consistent section.
            // Example used here: "Jwt" with keys: Issuer, Audience, SecretKey
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            builder.Services
                .AddAuthentication(options =>
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
                        ValidIssuer = issuer,
                        ValidAudience = audience,
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
            builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

            // ---------- Swagger ----------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-CommerceSystem API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // ---------- Pipeline ----------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Optional: Global minimal 500 handler for cleaner responses
            //app.UseExceptionHandler(cfg =>
            //{
            //    cfg.Run(async ctx =>
            //    {
            //        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            //        ctx.Response.ContentType = "application/problem+json";
            //        await ctx.Response.WriteAsJsonAsync(new { type = "about:blank", title = "Unexpected error", status = 500 });
            //    });
            //});

            app.UseHttpsRedirection();
            app.UseStaticFiles();        // for images
            app.UseAuthentication();
            app.UseAuthorization();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            app.MapControllers();

            Log.Information("E_CommerceSystem starting up");
            app.Run();
        }
    }
}

