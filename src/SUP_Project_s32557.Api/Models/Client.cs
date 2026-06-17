using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Models;

public class Client
{
    public int Id { get; set; }
    public ClientType Type { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LegalIdentifier { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public List<Contract> Contracts { get; set; } = new();
    public List<Subscription> Subscriptions { get; set; } = new();
}