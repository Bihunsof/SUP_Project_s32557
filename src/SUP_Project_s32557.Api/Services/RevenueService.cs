using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Services;

public interface IRevenueService
{
    Task<decimal> CurrentRevenuePlnAsync(int? productId);
    Task<decimal> PredictedRevenuePlnAsync(int? productId, int monthsAhead);
}

public class RevenueService : IRevenueService
{
    private readonly AppDbContext _db;
    public RevenueService(AppDbContext db) => _db = db;

    public async Task<decimal> CurrentRevenuePlnAsync(int? productId)
    {
        var contractRevenue = await _db.Contracts
            .Where(c => c.Status == ContractStatus.Paid && (productId == null || c.SoftwareProductId == productId))
            .SumAsync(c => (decimal?)c.FinalPrice) ?? 0m;

        var subscriptionRevenue = await _db.SubscriptionPayments
            .Include(p => p.Subscription).ThenInclude(s => s.SubscriptionPlan)
            .Where(p => productId == null || p.Subscription.SubscriptionPlan.SoftwareProductId == productId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        return Math.Round(contractRevenue + subscriptionRevenue, 2);
    }

    public async Task<decimal> PredictedRevenuePlnAsync(int? productId, int monthsAhead)
    {
        if (monthsAhead < 0 || monthsAhead > 60) throw new BusinessException("monthsAhead must be between 0 and 60.");
        var current = await CurrentRevenuePlnAsync(productId);

        var unsignedContracts = await _db.Contracts
            .Where(c => c.Status == ContractStatus.Offer && (productId == null || c.SoftwareProductId == productId))
            .SumAsync(c => (decimal?)c.FinalPrice) ?? 0m;

        var activeSubs = await _db.Subscriptions.Include(s => s.SubscriptionPlan)
            .Where(s => s.Status == SubscriptionStatus.Active && (productId == null || s.SubscriptionPlan.SoftwareProductId == productId))
            .ToListAsync();

        decimal renewals = 0m;
        foreach (var s in activeSubs)
        {
            var periods = Math.Max(0, monthsAhead / s.SubscriptionPlan.RenewalPeriodMonths);
            renewals += periods * s.SubscriptionPlan.PricePerPeriod;
        }
        return Math.Round(current + unsignedContracts + renewals, 2);
    }
}
