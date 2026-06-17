using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Models;
using SUP_Project_s32557.Api.Models.Enums;
using SUP_Project_s32557.Api.Auth;

namespace SUP_Project_s32557.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<SoftwareProduct> SoftwareProducts => Set<SoftwareProduct>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractPayment> ContractPayments => Set<ContractPayment>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Client>().HasIndex(x => x.LegalIdentifier).IsUnique();
        b.Entity<Employee>().HasIndex(x => x.Login).IsUnique();
        b.Entity<Contract>().Property(x => x.FinalPrice).HasPrecision(18, 2);
        b.Entity<Contract>().Property(x => x.BasePrice).HasPrecision(18, 2);
        b.Entity<ContractPayment>().Property(x => x.Amount).HasPrecision(18, 2);
        b.Entity<SubscriptionPlan>().Property(x => x.PricePerPeriod).HasPrecision(18, 2);
        b.Entity<SubscriptionPayment>().Property(x => x.Amount).HasPrecision(18, 2);
        b.Entity<SoftwareProduct>().Property(x => x.LicensePrice).HasPrecision(18, 2);
        b.Entity<Discount>().Property(x => x.Percentage).HasPrecision(5, 2);

        b.Entity<Client>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Contract>().HasQueryFilter(x => !x.IsDeleted);

        b.Entity<SoftwareProduct>().HasData(
            new SoftwareProduct { Id = 1, Name = "ABC Finance", Description = "System finansowy", CurrentVersion = "2026.1", Category = "Finanse", LicensePrice = 12000m },
            new SoftwareProduct { Id = 2, Name = "ABC Edu", Description = "Platforma edukacyjna", CurrentVersion = "5.4", Category = "Edukacja", LicensePrice = 8000m },
            new SoftwareProduct { Id = 3, Name = "ABC CRM", Description = "System CRM", CurrentVersion = "3.2", Category = "Sprzedaż", LicensePrice = 10000m }
        );
        b.Entity<Discount>().HasData(
            new Discount { Id = 1, Name = "Black Friday Contract", OfferType = OfferType.Contract, Percentage = 10m, ActiveFrom = new DateOnly(2026, 1, 1), ActiveTo = new DateOnly(2026, 12, 31) },
            new Discount { Id = 2, Name = "Subscription Promo", OfferType = OfferType.Subscription, Percentage = 10m, ActiveFrom = new DateOnly(2026, 1, 1), ActiveTo = new DateOnly(2026, 12, 31) }
        );
        b.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = 1, SoftwareProductId = 1, Name = "ABC Finance Monthly", RenewalPeriodMonths = 1, PricePerPeriod = 1200m },
            new SubscriptionPlan { Id = 2, SoftwareProductId = 2, Name = "ABC Edu Yearly", RenewalPeriodMonths = 12, PricePerPeriod = 7000m }
        );
        b.Entity<Employee>().HasData(
            new Employee { Id = 1, Login = "admin", PasswordHash = PasswordHasher.Hash("admin123"), Role = EmployeeRole.Admin },
            new Employee { Id = 2, Login = "user", PasswordHash = PasswordHasher.Hash("user123"), Role = EmployeeRole.User }
        );
    }
}
