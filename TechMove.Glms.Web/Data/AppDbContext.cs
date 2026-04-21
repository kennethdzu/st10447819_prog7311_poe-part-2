using Microsoft.EntityFrameworkCore;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<FreightContract> FreightContracts { get; set; }
    public DbSet<WarehousingContract> WarehousingContracts { get; set; }
    public DbSet<LastMileContract> LastMileContracts { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contract>()
            .HasDiscriminator<string>("ContractType")
            .HasValue<FreightContract>("Freight")
            .HasValue<WarehousingContract>("Warehousing")
            .HasValue<LastMileContract>("LastMile");

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(e => e.ContactDetails)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Region)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Link to Client. Delete contracts when a client is removed.
            entity.HasOne(e => e.Client)
                  .WithMany(c => c.Contracts)
                  .HasForeignKey(e => e.ClientId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("Draft");

            entity.Property(e => e.ServiceLevel)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.SignedAgreementPdfPath)
                  .HasMaxLength(255);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Delete service requests when a contract is removed.
            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.ServiceRequests)
                  .HasForeignKey(e => e.ContractId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("Pending");

            entity.Property(e => e.Cost)
                  .HasColumnType("decimal(18,2)");
        });
    }
}

