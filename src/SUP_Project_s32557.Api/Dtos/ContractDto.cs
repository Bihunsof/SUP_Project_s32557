namespace SUP_Project_s32557.Api.Dtos;

public record ContractDto(
    int Id,
    int ClientId,
    int SoftwareProductId,
    string SoftwareVersion,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal BasePrice,
    decimal DiscountPercentage,
    decimal FinalPrice,
    decimal PaidAmount,
    decimal RemainingAmount,
    string Status
);