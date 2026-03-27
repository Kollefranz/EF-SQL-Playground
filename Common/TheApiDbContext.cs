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
        x.Entity<ServerEntity>().HasKey(s => s.RowId);
        x.Entity<ServerEntity>().Property(s => s.RowId).ValueGeneratedOnAdd();
        x.Entity<ServerEntity>().HasIndex(s => s.Id).IsUnique();
        x.Entity<ServerEntity>().Property(s => s.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        x.Entity<ServerEntity>().Property(s => s.Hostname).HasMaxLength(256);
        x.Entity<ServerEntity>().Property(s => s.IpAddress).HasMaxLength(45);
        x.Entity<ServerEntity>().Property(s => s.OperatingSystem).HasMaxLength(100);
        x.Entity<ServerEntity>().Property(s => s.Status).HasMaxLength(50);
        x.Entity<ServerEntity>().Property(s => s.Environment).HasMaxLength(50);

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

        x.Entity<DiskEntity>()
            .HasOne(d => d.Server)
            .WithMany(s => s.Disks)
            .HasForeignKey(d => d.ServerId)
            .HasPrincipalKey(s => s.RowId);

        x.Entity<NetworkInterfaceEntity>()
            .HasOne(n => n.Server)
            .WithMany(s => s.NetworkInterfaces)
            .HasForeignKey(n => n.ServerId)
            .HasPrincipalKey(s => s.RowId);

        x.Entity<InstalledServiceEntity>()
            .HasOne(i => i.Server)
            .WithMany(s => s.InstalledServices)
            .HasForeignKey(i => i.ServerId)
            .HasPrincipalKey(s => s.RowId);

        x.Entity<ServerTagEntity>()
            .HasOne(t => t.Server)
            .WithMany(s => s.Tags)
            .HasForeignKey(t => t.ServerId)
            .HasPrincipalKey(s => s.RowId);

        x.Entity<NetworkInterfaceEntity>()
            .HasIndex(n => n.ServerId)
            .IncludeProperties(n => new { n.Id, n.Name, n.MacAddress, n.IpAddress, n.SubnetMask, n.VlanId, n.IsEnabled });

        x.Entity<DiskEntity>()
            .HasIndex(d => d.ServerId)
            .IncludeProperties(d => new { d.Id, d.MountPoint, d.CapacityGb, d.DiskType, d.UsedGb });

        x.Entity<ServerTagEntity>()
            .HasIndex(t => t.ServerId)
            .IncludeProperties(t => new { t.Id, t.Key, t.Value });

        x.Entity<InstalledServiceEntity>()
            .HasIndex(i => i.ServerId)
            .IncludeProperties(i => new { i.Id, i.Name, i.Version, i.Port, i.Status, i.InstalledAt });
    }
}
