
using Microsoft.EntityFrameworkCore;
using NotesApp.Domain.Entities;

namespace NotesApp.Infrastructure.Data;

public class NotesDbContext : DbContext
{
    public DbSet<Note> Notes { set; get; }
    public DbSet<Tag> Tags { get; set; }

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

    }
}