namespace SUP_Project_s32557.Api.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public int SoftwareProductId { get; set; }
    public SoftwareProduct SoftwareProduct { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int RenewalPeriodMonths { get; set; }
    public decimal PricePerPeriod { get; set; }
}