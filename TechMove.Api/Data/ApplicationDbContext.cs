
using Microsoft.EntityFrameworkCore;
using TechMove.Models;

namespace TechMove.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>()
            .Property(c => c.Name).HasMaxLength(150).IsRequired();

        modelBuilder.Entity<Client>()
            .Property(c => c.ContactDetails).HasMaxLength(250).IsRequired();

        modelBuilder.Entity<Client>()
            .Property(c => c.Region).HasMaxLength(100).IsRequired();

        modelBuilder.Entity<Contract>()
            .Property(c => c.Status).HasMaxLength(20).IsRequired();

        modelBuilder.Entity<Contract>()
            .Property(c => c.ServiceLevel).HasMaxLength(100).IsRequired();

        modelBuilder.Entity<Contract>()
            .Property(c => c.SignedAgreementPath).HasMaxLength(500);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Client)
            .WithMany(c => c.Contracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.Description).HasMaxLength(500).IsRequired();

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.CostUsd).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.ExchangeRate).HasColumnType("decimal(18,6)");

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.CostZar).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<ServiceRequest>()
            .Property(s => s.Status).HasMaxLength(30).IsRequired();

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(s => s.Contract)
            .WithMany(c => c.ServiceRequests)
            .HasForeignKey(s => s.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
