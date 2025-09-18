using Example.Hubs;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Security.Claims;
using TmaAuth;

namespace Example;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Add services to the container.
        builder.Services.AddAuthentication()
        .AddTelegramMiniAppToken(options =>
        {
            options.BotToken = builder.Configuration["TelegramOptions:BotToken"];
            options.Events = new TmaEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/balance"))
                    {
                        context.InitDataRaw = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });


        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(TmaPolicy.TmaUserPremiumPolicy, policy =>
            {
                policy.AddAuthenticationSchemes(TmaTokenDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimTypes.NameIdentifier);
                policy.RequireClaim(TmaClaim.IsPremium, "true");  // user must have premium account to access
            });
        });
        builder.Services.AddSignalR();
        builder.Services.AddScoped<IBalanceNotificationService, BalanceHub>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo { Title = nameof(Assembly), Version = "1" });
            s.ResolveConflictingActions(x => x.First());
            s.AddSecurityDefinition("tma", new OpenApiSecurityScheme
            {
                Description = "TMA Authorization header using the scheme. Example: \"Authorization: TMA {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "tma" // This should match the scheme used in the authentication handler
            });

            var apiScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header,
            };
            // Add a security requirement for the TMA scheme
            s.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "tma"
                        },
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
            });
            s.OperationFilter<AddAuthHeaderOperationFilter>();
        });

        var app = builder.Build();
        app.UseCors("AllowAll");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();
        app.MapHub<BalanceHub>("/balance");

        app.Run();
    }
}

public class AddAuthHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        var authHeader = operation.Parameters.FirstOrDefault(p => p.Name == "Authorization");
        if (authHeader != null)
        {
            authHeader.Description = "TMA {token}";
        }
    }
}