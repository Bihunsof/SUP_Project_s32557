namespace SUP_Project_s32557.Api.Dtos;

public record SubscriptionPlanDto(
    int Id,
    int SoftwareProductId,
    string Name,
    int RenewalPeriodMonths,
    decimal PricePerPeriod);
