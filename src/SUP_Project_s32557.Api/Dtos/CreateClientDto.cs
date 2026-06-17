using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Dtos;

public record CreateClientDto(
    ClientType Type,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string Address,
    string Email,
    string Phone,
    string LegalIdentifier);
