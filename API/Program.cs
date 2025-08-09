
using API.Data;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.Applicationservices(builder.Configuration);
            builder.Services.IdentityServices(builder.Configuration);
            builder.Services.AddSignalR();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("myPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:4200");
                });
            });

            var app = builder.Build();
            #region Update-Database
            var scope = app.Services.CreateScope();
            var services=scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<DataContext>();
                var userManger=services.GetRequiredService<UserManager<AppUser>>();
                var roleManger=services.GetRequiredService<RoleManager<AppRole>>();
                await dbContext.Database.MigrateAsync();
                await Seed.SeedUsers(userManger, roleManger);
            }
            catch (Exception ex)
            {
                var logger=services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An Error occurred during migration");
            }
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseCors("myPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<PresenceHub>("hubs/presence");
            app.MapHub<MessageHub>("hubs/message");
            app.Run();
        }
    }
}
