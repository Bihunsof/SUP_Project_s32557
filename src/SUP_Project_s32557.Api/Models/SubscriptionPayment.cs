namespace SUP_Project_s32557.Api.Models;

public class SubscriptionPayment
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
}