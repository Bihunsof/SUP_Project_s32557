namespace SUP_Project_s32557.Api.Dtos;

public record SubscriptionDto(
    int Id,
    int ClientId,
    int SubscriptionPlanId,
    DateOnly CurrentPeriodStart,
    DateOnly CurrentPeriodEnd,
    string Status);
