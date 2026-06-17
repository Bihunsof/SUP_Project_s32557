using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Models;
using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Services;

public interface IPricingService
{
    Task<decimal> DiscountPercentAsync(int clientId, OfferType offerType, DateOnly date);
    Task<bool> IsReturningClientAsync(int clientId);
}

public class PricingService : IPricingService
{
    private readonly AppDbContext _db;
    public PricingService(AppDbContext db) => _db = db;

    public async Task<bool> IsReturningClientAsync(int clientId)
    {
        var hadContract = await _db.Contracts.IgnoreQueryFilters().AnyAsync(c => c.ClientId == clientId && c.Status == ContractStatus.Paid);
        var hadSubscription = await _db.Subscriptions.AnyAsync(s => s.ClientId == clientId);
        return hadContract || hadSubscription;
    }

    public async Task<decimal> DiscountPercentAsync(int clientId, OfferType offerType, DateOnly date)
    {
        var bestPromo = await _db.Discounts
            .Where(d => d.OfferType == offerType && d.ActiveFrom <= date && d.ActiveTo >= date)
            .MaxAsync(d => (decimal?)d.Percentage) ?? 0m;

        var loyalty = await IsReturningClientAsync(clientId) ? 5m : 0m;
        return bestPromo + loyalty;
    }
}
