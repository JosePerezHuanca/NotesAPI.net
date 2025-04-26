using Microsoft.EntityFrameworkCore;
using NotesAPI.Data;
using DotNetEnv;

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
                options.UseMySql(Environment.GetEnvironmentVariable("CONNECTION_STRING"), ServerVersion.AutoDetect(Environment.GetEnvironmentVariable("CONNECTION_STRING")));
            }
            );
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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
