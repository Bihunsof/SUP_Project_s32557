namespace SUP_Project_s32557.Api.Models;

public class ContractPayment
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
}