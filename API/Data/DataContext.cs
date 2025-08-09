
using API.Entities;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace API.Data
{
    public class DataContext:IdentityDbContext<AppUser,AppRole,int,
        IdentityUserClaim<int>,AppUserRole,IdentityUserLogin<int>,
        IdentityRoleClaim<int>,IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>()
                 .HasMany(ur => ur.UserRoles)
                 .WithOne(u => u.User)
                 .HasForeignKey(u => u.UserId)
                 .IsRequired();
            modelBuilder.Entity<AppRole>()
                 .HasMany(r => r.UserRoles)
                 .WithOne(ur => ur.Role)
                 .HasForeignKey(r => r.RoleId)
                 .IsRequired();

            modelBuilder.Entity<UserLike>()
                .HasKey(K => new { K.SourceUserId, K.LikedUserId });
            modelBuilder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLike>()
              .HasOne(s => s.LikedUser)
              .WithMany(l => l.LikedByUsers)
              .HasForeignKey(s => s.LikedUserId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(u => u.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
              .HasOne(u => u.Sender)
              .WithMany(u => u.MessagesSent)
              .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.ApplyUtcDateTimeConverter();
        }
    }
    public static class UtcDateAnnotation
    {
        private const string IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime,DateTime> UtcConverter=
            new ValueConverter<DateTime,DateTime>(v=>v,v=>DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
            new ValueConverter<DateTime?, DateTime?>(v => v, v => v ==null?v:DateTime.SpecifyKind(v.Value,DateTimeKind.Utc));
        public static PropertyBuilder<TProperty> ISUtc<TProperty>(this PropertyBuilder<TProperty> builder,Boolean isUtc=true)=>
            builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static Boolean IsUtc(this IMutableProperty property) =>
            ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;


        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if(!property.IsUtc())
                    {
                        continue;
                    }
                    if(property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(UtcConverter);
                    }
                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcNullableConverter);
                    }
                }
            }
        }
    }
}
