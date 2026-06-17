namespace SUP_Project_s32557.Api.Dtos;

public record CreateContractDto(
    int ClientId,
    int SoftwareProductId,
    DateOnly StartDate,
    DateOnly EndDate,
    int AdditionalSupportYears);
