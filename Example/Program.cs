using Example.Hubs;
using System.Security.Claims;
using TmaAuth;

namespace Example
{
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
            builder.Services.AddSwaggerGen();

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
}
