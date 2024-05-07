
using System.Security.Claims;
using TmaAuth;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthentication()
            .AddTelegramMiniAppToken(options =>
            {
                options.BotToken = builder.Configuration["TelegramOptions:BotToken"];
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(TmaPolicy.TmaUserPremiumPolicy, policy =>
                {
                    policy.AddAuthenticationSchemes(TmaTokenDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.NameIdentifier);
                    policy.RequireClaim(TmaClaim.IsPremium, "true");  // Требуем, чтобы пользователь был премиум-пользователем
                });
            });



            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
