using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common;

public class TheApiDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ServerEntity> Servers => Set<ServerEntity>();
    public DbSet<DiskEntity> Disks => Set<DiskEntity>();
    public DbSet<NetworkInterfaceEntity> NetworkInterfaces => Set<NetworkInterfaceEntity>();
    public DbSet<InstalledServiceEntity> InstalledServices => Set<InstalledServiceEntity>();
    public DbSet<ServerTagEntity> ServerTags => Set<ServerTagEntity>();

    protected override void OnModelCreating(ModelBuilder x)
    {
        x.Entity<ServerEntity>()
            .Property(s => s.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        x.Entity<DiskEntity>()
            .Property(d => d.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        x.Entity<NetworkInterfaceEntity>()
            .Property(n => n.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        x.Entity<InstalledServiceEntity>()
            .Property(i => i.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        x.Entity<ServerTagEntity>()
            .Property(t => t.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");
    }
}
