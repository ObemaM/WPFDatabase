using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFDatabase.Models;

namespace WPFDatabase.Data;

public class AppDbContext : DbContext
{
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<SmartphoneModel> SmartphoneModels => Set<SmartphoneModel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ActionLog> ActionLogs => Set<ActionLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Здесь описываем структуру таблиц, ограничения и поведение связей при удалении
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.ToTable("Series");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Segment)
                .IsRequired()
                .HasMaxLength(50);

            // Restrict не дает удалить бренд, пока на него ссылаются дочерние серии
            entity.HasOne(x => x.Brand)
                .WithMany(x => x.Series)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BrandId, x.Name })
                .IsUnique();
        });

        modelBuilder.Entity<SmartphoneModel>(entity =>
        {
            entity.ToTable("SmartphoneModels");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(x => x.Price)
                .HasPrecision(10, 2);

            // Restrict не дает удалить серию, пока в ней еще есть модели смартфонов
            entity.HasOne(x => x.Series)
                .WithMany(x => x.SmartphoneModels)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.SeriesId, x.Name })
                .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Login)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(x => x.Salt)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(x => x.RegisteredAt)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => x.Login)
                .IsUnique();
        });

        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.ToTable("ActionLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserLoginSnapshot)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.ActionType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Details)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            // При удалении пользователя логи сохраняются, а внешний ключ на пользователя становится пустым
            entity.HasOne(x => x.User)
                .WithMany(x => x.ActionLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
