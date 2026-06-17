using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Dtos;
using SUP_Project_s32557.Api.Models;
using SUP_Project_s32557.Api.Models.Enums;
using SUP_Project_s32557.Api.Services;
using Xunit;

namespace SUP_Project_s32557.Tests;

public class ContractServiceTests
{
    private static AppDbContext Db()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(opts);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        db.Clients.AddRange(
            new Client
            {
                Id = 1,
                Type = ClientType.Individual,
                FirstName = "Jan",
                LastName = "Kowalski",
                Address = "Warszawa",
                Email = "jan@test.pl",
                Phone = "123456789",
                LegalIdentifier = "12345678901"
            },
            new Client
            {
                Id = 2,
                Type = ClientType.Company,
                CompanyName = "ABC Sp. z o.o.",
                Address = "Krakow",
                Email = "abc@test.pl",
                Phone = "987654321",
                LegalIdentifier = "0000123456"
            }
        );

        db.SoftwareProducts.AddRange(
            new SoftwareProduct
            {
                Id = 1,
                Name = "ABC CRM",
                Description = "CRM",
                Category = "Sales",
                CurrentVersion = "1.0",
                LicensePrice = 10000m
            },
            new SoftwareProduct
            {
                Id = 2,
                Name = "ABC Edu",
                Description = "Education platform",
                Category = "Education",
                CurrentVersion = "2.0",
                LicensePrice = 8000m
            }
        );

        db.Discounts.AddRange(
            new Discount
            {
                Id = 1,
                Name = "Contract promo",
                OfferType = OfferType.Contract,
                Percentage = 10m,
                ActiveFrom = today.AddDays(-30),
                ActiveTo = today.AddDays(30)
            },
            new Discount
            {
                Id = 2,
                Name = "Subscription promo",
                OfferType = OfferType.Subscription,
                Percentage = 10m,
                ActiveFrom = today.AddDays(-30),
                ActiveTo = today.AddDays(30)
            }
        );

        db.SubscriptionPlans.AddRange(
            new SubscriptionPlan
            {
                Id = 1,
                SoftwareProductId = 1,
                Name = "Monthly plan",
                RenewalPeriodMonths = 1,
                PricePerPeriod = 1200m
            },
            new SubscriptionPlan
            {
                Id = 2,
                SoftwareProductId = 2,
                Name = "Yearly plan",
                RenewalPeriodMonths = 12,
                PricePerPeriod = 7000m
            }
        );

        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Contract_Uses_Best_Discount_And_Support_Cost()
    {
        await using var db = Db();
        var service = new ContractService(db, new PricingService(db));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.CreateAsync(new CreateContractDto(1, 1, today, today.AddDays(10), 2));

        Assert.Equal(12000m, result.BasePrice);
        Assert.Equal(10m, result.DiscountPercentage);
        Assert.Equal(10800m, result.FinalPrice);
    }

    [Fact]
    public async Task Contract_Shorter_Than_3_Days_Is_Rejected()
    {
        await using var db = Db();
        var service = new ContractService(db, new PricingService(db));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateContractDto(1, 1, today, today.AddDays(1), 0)));
    }

    [Fact]
    public async Task Contract_Longer_Than_30_Days_Is_Rejected()
    {
        await using var db = Db();
        var service = new ContractService(db, new PricingService(db));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateContractDto(1, 1, today, today.AddDays(31), 0)));
    }

    [Fact]
    public async Task Contract_Overpayment_Is_Rejected()
    {
        await using var db = Db();
        var service = new ContractService(db, new PricingService(db));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contract = await service.CreateAsync(new CreateContractDto(1, 1, today, today.AddDays(10), 0));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.PayAsync(contract.Id, new PayContractDto(contract.FinalPrice + 1m)));
    }

    [Fact]
    public async Task Contract_Full_Payment_Sets_Status_Paid()
    {
        await using var db = Db();
        var service = new ContractService(db, new PricingService(db));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contract = await service.CreateAsync(new CreateContractDto(1, 1, today, today.AddDays(10), 0));
        var paid = await service.PayAsync(contract.Id, new PayContractDto(contract.FinalPrice));

        Assert.Equal("Paid", paid.Status);
        Assert.Equal(contract.FinalPrice, paid.PaidAmount);
        Assert.Equal(0m, paid.RemainingAmount);
    }

    [Fact]
    public async Task Company_Client_Delete_Is_Rejected()
    {
        await using var db = Db();
        var service = new ClientService(db);

        await Assert.ThrowsAsync<BusinessException>(() => service.DeleteAsync(2));
    }

    [Fact]
    public async Task Individual_Client_Delete_Anonymizes_And_Hides_Client()
    {
        await using var db = Db();
        var service = new ClientService(db);

        await service.DeleteAsync(1);

        var deletedClient = await db.Clients.IgnoreQueryFilters().SingleAsync(c => c.Id == 1);
        var visibleClients = await db.Clients.ToListAsync();

        Assert.True(deletedClient.IsDeleted);
        Assert.Equal("DELETED", deletedClient.FirstName);
        Assert.Equal("DELETED", deletedClient.LastName);
        Assert.Equal("DELETED_1", deletedClient.LegalIdentifier);
        Assert.DoesNotContain(visibleClients, c => c.Id == 1);
    }

    [Fact]
    public async Task Monthly_Subscription_Renew_After_End_Extends_By_One_Month()
    {
        await using var db = Db();
        var service = new SubscriptionService(db, new PricingService(db));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldStart = today.AddMonths(-1);
        var oldEnd = today.AddDays(-1);

        db.Subscriptions.Add(new Subscription
        {
            Id = 1,
            ClientId = 1,
            SubscriptionPlanId = 1,
            CurrentPeriodStart = oldStart,
            CurrentPeriodEnd = oldEnd,
            Status = SubscriptionStatus.Active,
            Payments = new List<SubscriptionPayment>
            {
                new() { Amount = 1080m, PeriodStart = oldStart, PeriodEnd = oldEnd }
            }
        });
        await db.SaveChangesAsync();

        var result = await service.RenewAsync(1, new PaySubscriptionDto(1020m));

        Assert.Equal(today, result.CurrentPeriodStart);
        Assert.Equal(today.AddMonths(1).AddDays(-1), result.CurrentPeriodEnd);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task Yearly_Subscription_Renew_After_End_Extends_By_One_Year()
    {
        await using var db = Db();
        var service = new SubscriptionService(db, new PricingService(db));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldStart = today.AddYears(-1);
        var oldEnd = today.AddDays(-1);

        db.Subscriptions.Add(new Subscription
        {
            Id = 1,
            ClientId = 1,
            SubscriptionPlanId = 2,
            CurrentPeriodStart = oldStart,
            CurrentPeriodEnd = oldEnd,
            Status = SubscriptionStatus.Active,
            Payments = new List<SubscriptionPayment>
            {
                new() { Amount = 6300m, PeriodStart = oldStart, PeriodEnd = oldEnd }
            }
        });
        await db.SaveChangesAsync();

        var result = await service.RenewAsync(1, new PaySubscriptionDto(5950m));

        Assert.Equal(today, result.CurrentPeriodStart);
        Assert.Equal(today.AddMonths(12).AddDays(-1), result.CurrentPeriodEnd);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task Subscription_Renew_After_Deadline_Cancels_Subscription()
    {
        await using var db = Db();
        var service = new SubscriptionService(db, new PricingService(db));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldEnd = today.AddDays(-8);
        var oldStart = oldEnd.AddMonths(-1).AddDays(1);

        db.Subscriptions.Add(new Subscription
        {
            Id = 1,
            ClientId = 1,
            SubscriptionPlanId = 1,
            CurrentPeriodStart = oldStart,
            CurrentPeriodEnd = oldEnd,
            Status = SubscriptionStatus.Active,
            Payments = new List<SubscriptionPayment>
            {
                new() { Amount = 1080m, PeriodStart = oldStart, PeriodEnd = oldEnd }
            }
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessException>(() => service.RenewAsync(1, new PaySubscriptionDto(1020m)));

        var subscription = await db.Subscriptions.SingleAsync(s => s.Id == 1);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }
}
