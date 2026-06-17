using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Models;
using SUP_Project_s32557.Api.Models.Enums;
using SUP_Project_s32557.Api.Dtos;

namespace SUP_Project_s32557.Api.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDto> BuyAsync(BuySubscriptionDto dto);
    Task<SubscriptionDto> RenewAsync(int subscriptionId, PaySubscriptionDto dto);
}

public class SubscriptionService : ISubscriptionService
{
    private const int RenewalPaymentDeadlineDays = 7;
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;
    public SubscriptionService(AppDbContext db, IPricingService pricing) { _db = db; _pricing = pricing; }

    public async Task<SubscriptionDto> BuyAsync(BuySubscriptionDto dto)
    {
        var client = await _db.Clients.FindAsync(dto.ClientId) ?? throw new BusinessException("Client not found.");
        var plan = await _db.SubscriptionPlans.FindAsync(dto.SubscriptionPlanId) ?? throw new BusinessException("Subscription plan not found.");
        if (plan.RenewalPeriodMonths is < 1 or > 24) throw new BusinessException("Renewal period must be between 1 month and 2 years.");

        var active = await _db.Subscriptions.Include(s => s.SubscriptionPlan)
            .AnyAsync(s => s.ClientId == client.Id && s.SubscriptionPlan.SoftwareProductId == plan.SoftwareProductId && s.Status == SubscriptionStatus.Active);
        if (active) throw new BusinessException("Client already has active subscription for this product.");

        var start = dto.StartDate;
        var end = start.AddMonths(plan.RenewalPeriodMonths).AddDays(-1);
        var discount = await _pricing.DiscountPercentAsync(client.Id, OfferType.Subscription, start);
        var amount = Math.Round(plan.PricePerPeriod * (1m - discount / 100m), 2);
        var subscription = new Subscription
        {
            ClientId = client.Id,
            SubscriptionPlanId = plan.Id,
            CurrentPeriodStart = start,
            CurrentPeriodEnd = end,
            Payments = new List<SubscriptionPayment> { new() { Amount = amount, PeriodStart = start, PeriodEnd = end } }
        };
        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync();
        return ToDto(subscription);
    }

    public async Task<SubscriptionDto> RenewAsync(int subscriptionId, PaySubscriptionDto dto)
    {
        var sub = await _db.Subscriptions
                      .Include(s => s.SubscriptionPlan)
                      .Include(s => s.Payments)
                      .SingleOrDefaultAsync(s => s.Id == subscriptionId)
                  ?? throw new BusinessException("Subscription not found.");

        if (sub.Status != SubscriptionStatus.Active)
            throw new BusinessException("Subscription is not active.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (today <= sub.CurrentPeriodEnd)
        {
            throw new BusinessException("Current subscription period has not ended yet.");
        }

        var paymentDeadline = sub.CurrentPeriodEnd.AddDays(RenewalPaymentDeadlineDays);

        if (today > paymentDeadline)
        {
            sub.Status = SubscriptionStatus.Cancelled;
            await _db.SaveChangesAsync();

            throw new BusinessException("Payment deadline has passed. Subscription has been cancelled.");
        }

        var nextStart = sub.CurrentPeriodEnd.AddDays(1);
        var nextEnd = nextStart
            .AddMonths(sub.SubscriptionPlan.RenewalPeriodMonths)
            .AddDays(-1);

        var alreadyPaid = sub.Payments.Any(p =>
            p.PeriodStart == nextStart &&
            p.PeriodEnd == nextEnd);

        if (alreadyPaid)
        {
            throw new BusinessException("Next renewal period is already paid.");
        }

        var discount = await _pricing.DiscountPercentAsync(sub.ClientId, OfferType.Subscription, nextStart);
        var due = Math.Round(sub.SubscriptionPlan.PricePerPeriod * (1m - discount / 100m), 2);

        if (dto.Amount != due)
            throw new BusinessException($"Payment must be exactly {due} PLN.");

        sub.Payments.Add(new SubscriptionPayment
        {
            Amount = dto.Amount,
            PeriodStart = nextStart,
            PeriodEnd = nextEnd,
            PaidAtUtc = DateTime.UtcNow
        });

        sub.CurrentPeriodStart = nextStart;
        sub.CurrentPeriodEnd = nextEnd;

        await _db.SaveChangesAsync();

        return ToDto(sub);
    }

    private static SubscriptionDto ToDto(Subscription s) => new(s.Id, s.ClientId, s.SubscriptionPlanId, s.CurrentPeriodStart, s.CurrentPeriodEnd, s.Status.ToString());
}
