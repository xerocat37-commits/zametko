using Microsoft.EntityFrameworkCore;
using ZAMETKI.Api.Domain;

namespace ZAMETKI.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired();
            e.Property(x => x.Role).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.HasIndex(x => x.FullName);
        });

        b.Entity<Group>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.TeacherId).IsRequired();
            e.HasIndex(x => x.TeacherId);
            e.HasIndex(x => x.Name);
        });

        b.Entity<GroupMember>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.GroupId).IsRequired();
            e.Property(x => x.StudentFullName).IsRequired();
            e.Property(x => x.Status).IsRequired();
            e.HasIndex(x => x.GroupId);
            e.HasIndex(x => x.StudentId);
            e.HasIndex(x => new { x.GroupId, x.StudentFullName });
        });

        b.Entity<Note>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.OwnerId).IsRequired();
            e.Property(x => x.NoteType).IsRequired();
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => x.NoteType);
            e.HasIndex(x => x.TargetGroupId);
        });

        b.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired();
            e.HasIndex(x => x.UserId);
        });
    }
}
