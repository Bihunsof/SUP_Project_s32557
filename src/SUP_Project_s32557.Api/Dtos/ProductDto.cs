namespace SUP_Project_s32557.Api.Dtos;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    string CurrentVersion,
    string Category,
    decimal LicensePrice);
