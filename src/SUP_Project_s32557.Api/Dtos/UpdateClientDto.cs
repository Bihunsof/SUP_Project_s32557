namespace SUP_Project_s32557.Api.Dtos;

public record UpdateClientDto(
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string Address,
    string Email,
    string Phone);
