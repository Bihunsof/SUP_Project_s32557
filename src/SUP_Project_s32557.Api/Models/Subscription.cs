using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Models;

public class Subscription
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public DateOnly CurrentPeriodStart { get; set; }
    public DateOnly CurrentPeriodEnd { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public List<SubscriptionPayment> Payments { get; set; } = new();
}