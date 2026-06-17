using SUP_Project_s32557.Api.Models.Enums;

namespace SUP_Project_s32557.Api.Models;

public class Contract
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int SoftwareProductId { get; set; }
    public SoftwareProduct SoftwareProduct { get; set; } = null!;
    public string SoftwareVersion { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int AdditionalSupportYears { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal FinalPrice { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Offer;
    public bool IsDeleted { get; set; }
    public DateTime? SignedAtUtc { get; set; }
    public List<ContractPayment> Payments { get; set; } = new();
}