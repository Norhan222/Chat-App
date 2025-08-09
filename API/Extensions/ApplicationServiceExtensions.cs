using API.Data;
using API.Helper;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection Applicationservices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<LogUserActivity>(); 
            services.AddAutoMapper(typeof(AutoMapperProfile));
       
            return services;
        }
    }
}
