
using Microsoft.EntityFrameworkCore;
using NotesApp.Domain.Entities;

namespace NotesApp.Infrastructure.Data;

public class NotesDbContext : DbContext
{
    public DbSet<Note> Notes { set; get; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>().HasKey(note => note.Id);
        modelBuilder.Entity<Note>().Property(note => note.Name).IsRequired().HasMaxLength(100);

        modelBuilder.Entity<Tag>().HasKey(tag => tag.Id);
        modelBuilder.Entity<Tag>().Property(tag => tag.Name).IsRequired().HasMaxLength(100);

        modelBuilder.Entity<Note>()
            .HasMany(note => note.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("NoteTags"));

        modelBuilder.Entity<User>().HasKey(user => user.Id);
        modelBuilder.Entity<User>().Property(user => user.Username).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<User>().Property(user => user.PasswordHash).IsRequired();

        modelBuilder.Entity<RefreshToken>().HasKey(rt => rt.Id);
        modelBuilder.Entity<RefreshToken>().Property(rt => rt.UserId).IsRequired();
        modelBuilder.Entity<RefreshToken>().Property(rt => rt.Token).IsRequired();
        modelBuilder.Entity<RefreshToken>().Property(rt => rt.ExpiryDate).IsRequired();
        modelBuilder.Entity<RefreshToken>().HasOne(rt => rt.User).WithMany().HasForeignKey(rt => rt.UserId);
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
    }
}