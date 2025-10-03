using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using NotesAPI.Repository;
using NotesAPI.Services;
using System.Threading.Channels;
using NotesAPI.Email;

namespace NotesAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Env.Load();

            // Add services to the container.
            builder.Services.AddDbContext<NoteContext>(options =>
            {
                options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
            }
            );
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<INoteRepository, NoteRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            // Channel singleton
            var emailChannel = Channel.CreateUnbounded<EmailRequest>();
            builder.Services.AddSingleton(emailChannel);
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddHostedService<EmailBackgroundService>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("TOKEN_ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("TOKEN_AUDIENCE"),
                    IssuerSigningKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")))
                };
            });
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddControllers()
            //Lo siguiente es para que las respuestas json no muestren valores null como DateTime? UpdatedAtt
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.WebHost.UseUrls("http://*:5172");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<NoteContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
