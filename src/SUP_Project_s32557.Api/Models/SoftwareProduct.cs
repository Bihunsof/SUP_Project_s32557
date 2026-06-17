namespace SUP_Project_s32557.Api.Models;

public class SoftwareProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal LicensePrice { get; set; }
    public List<SubscriptionPlan> SubscriptionPlans { get; set; } = new();
}