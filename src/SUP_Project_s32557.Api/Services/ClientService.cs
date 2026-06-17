using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Models;
using SUP_Project_s32557.Api.Models.Enums;
using SUP_Project_s32557.Api.Dtos;
using System.Text.RegularExpressions;

namespace SUP_Project_s32557.Api.Services;

public interface IClientService
{
    Task<List<ClientDto>> GetAsync();
    Task<ClientDto> CreateAsync(CreateClientDto dto);
    Task<ClientDto> UpdateAsync(int id, UpdateClientDto dto);
    Task DeleteAsync(int id);
}

public class ClientService : IClientService
{
    private readonly AppDbContext _db;
    public ClientService(AppDbContext db) => _db = db;

    public async Task<List<ClientDto>> GetAsync() => await _db.Clients.Select(ToDtoExpr).ToListAsync();

    public async Task<ClientDto> CreateAsync(CreateClientDto dto)
    {
        ValidateClient(dto.Type, dto.FirstName, dto.LastName, dto.CompanyName, dto.Address, dto.Email, dto.Phone, dto.LegalIdentifier);
        if (await _db.Clients.IgnoreQueryFilters().AnyAsync(x => x.LegalIdentifier == dto.LegalIdentifier))
            throw new BusinessException("Client with this PESEL/KRS already exists.");

        var client = new Client
        {
            Type = dto.Type, FirstName = dto.FirstName, LastName = dto.LastName, CompanyName = dto.CompanyName,
            Address = dto.Address, Email = dto.Email, Phone = dto.Phone, LegalIdentifier = dto.LegalIdentifier
        };
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return ToDto(client);
    }

    public async Task<ClientDto> UpdateAsync(int id, UpdateClientDto dto)
    {
        var client = await _db.Clients.FindAsync(id) ?? throw new BusinessException("Client not found.");
        ValidateClient(client.Type, dto.FirstName, dto.LastName, dto.CompanyName, dto.Address, dto.Email, dto.Phone, client.LegalIdentifier);
        client.FirstName = dto.FirstName;
        client.LastName = dto.LastName;
        client.CompanyName = dto.CompanyName;
        client.Address = dto.Address;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        await _db.SaveChangesAsync();
        return ToDto(client);
    }

    public async Task DeleteAsync(int id)
    {
        var client = await _db.Clients.FindAsync(id) ?? throw new BusinessException("Client not found.");
        if (client.Type == ClientType.Company)
            throw new BusinessException("Company clients cannot be deleted.");

        client.IsDeleted = true;
        client.FirstName = "DELETED";
        client.LastName = "DELETED";
        client.Address = "DELETED";
        client.Email = $"deleted-{client.Id}@example.invalid";
        client.Phone = "DELETED";
        client.LegalIdentifier = $"DELETED_{client.Id}";
        await _db.SaveChangesAsync();
    }

    private static void ValidateClient(ClientType type, string? firstName, string? lastName, string? companyName, string address, string email, string phone, string legalId)
    {
        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(legalId))
            throw new BusinessException("Address, email, phone and legal identifier are required.");
        if (type == ClientType.Individual && !Regex.IsMatch(legalId, @"^\d{11}$"))
            throw new BusinessException("PESEL must contain exactly 11 digits.");
        if (type == ClientType.Company && !Regex.IsMatch(legalId, @"^\d{10}$"))
            throw new BusinessException("KRS must contain exactly 10 digits.");
        if (type == ClientType.Individual && (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)))
            throw new BusinessException("Individual client requires first name and last name.");
        if (type == ClientType.Company && string.IsNullOrWhiteSpace(companyName))
            throw new BusinessException("Company client requires company name.");
    }

    private static readonly System.Linq.Expressions.Expression<Func<Client, ClientDto>> ToDtoExpr = c => new ClientDto(c.Id, c.Type, c.FirstName, c.LastName, c.CompanyName, c.Address, c.Email, c.Phone, c.LegalIdentifier);
    private static ClientDto ToDto(Client c) => new(c.Id, c.Type, c.FirstName, c.LastName, c.CompanyName, c.Address, c.Email, c.Phone, c.LegalIdentifier);
}
