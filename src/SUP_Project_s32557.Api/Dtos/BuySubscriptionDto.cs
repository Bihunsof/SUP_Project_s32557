namespace SUP_Project_s32557.Api.Dtos;

public record BuySubscriptionDto(
    int ClientId,
    int SubscriptionPlanId,
    DateOnly StartDate);
