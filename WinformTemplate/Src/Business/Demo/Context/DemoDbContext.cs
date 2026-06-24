using Microsoft.EntityFrameworkCore;
using WinformTemplate.Business.Demo.Model;
using WinformTemplate.Common.DataAccess;
using WinformTemplate.Serialize;

namespace WinformTemplate.Business.Demo.Context;

public sealed class DemoDbContext : DbContext
{
    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    public DbSet<DemoNote> DemoNotes => Set<DemoNote>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        EfDbContextOptions.UseConfiguredDatabase(optionsBuilder, GlobalProjectConfig.Instance.Config?.Ef, "Demo");
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DemoNote>(entity =>
        {
            entity.HasIndex(note => note.Title);
            entity.HasIndex(note => note.Pinned);
            entity.HasIndex(note => note.CreateAt);
            entity.Property(note => note.Title).HasMaxLength(120).IsRequired();
            entity.Property(note => note.Content).HasMaxLength(4000);
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var now = DateTime.Now;
        foreach (var entry in ChangeTracker.Entries<DemoNote>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreateAt = entry.Entity.CreateAt == default ? now : entry.Entity.CreateAt;
                entry.Entity.UpdateAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateAt = now;
            }
        }
    }
}
