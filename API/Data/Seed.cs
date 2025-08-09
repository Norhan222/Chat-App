using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;
            var userDate = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userDate);
            if(users==null) return;
            var roles = new List<AppRole>
            {
                new AppRole{Name="Admin"},
                new AppRole{Name="Moderator"},
                new AppRole{Name="Member"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                await userManager.CreateAsync(user, "P@ssw0rd");
                await userManager.AddToRoleAsync(user, "Member");


            }
                var admin = new AppUser
                {
                    UserName = "admin"
                };
                await userManager.CreateAsync(admin, "P@ssw0rd");
                await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
            

        }
    }
}
