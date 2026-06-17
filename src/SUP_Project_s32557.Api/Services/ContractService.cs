using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Models;
using SUP_Project_s32557.Api.Models.Enums;
using SUP_Project_s32557.Api.Dtos;

namespace SUP_Project_s32557.Api.Services;

public interface IContractService
{
    Task<ContractDto> CreateAsync(CreateContractDto dto);
    Task<ContractDto> PayAsync(int contractId, PayContractDto dto);
    Task DeleteAsync(int contractId);
}

public class ContractService : IContractService
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;
    public ContractService(AppDbContext db, IPricingService pricing) { _db = db; _pricing = pricing; }

    public async Task<ContractDto> CreateAsync(CreateContractDto dto)
    {
        var days = dto.EndDate.DayNumber - dto.StartDate.DayNumber;
        if (days < 3 || days > 30) throw new BusinessException("Contract duration must be between 3 and 30 days.");
        if (dto.AdditionalSupportYears is < 0 or > 3) throw new BusinessException("Additional support can be 0, 1, 2 or 3 years.");

        var client = await _db.Clients.FindAsync(dto.ClientId) ?? throw new BusinessException("Client not found.");
        var product = await _db.SoftwareProducts.FindAsync(dto.SoftwareProductId) ?? throw new BusinessException("Software product not found.");

        var hasActiveContract = await _db.Contracts.AnyAsync(c => c.ClientId == dto.ClientId && c.SoftwareProductId == dto.SoftwareProductId && c.Status == ContractStatus.Offer && c.EndDate >= dto.StartDate);
        var hasActiveSubscription = await _db.Subscriptions.Include(s => s.SubscriptionPlan).AnyAsync(s => s.ClientId == dto.ClientId && s.SubscriptionPlan.SoftwareProductId == dto.SoftwareProductId && s.Status == SubscriptionStatus.Active);
        if (hasActiveContract || hasActiveSubscription)
            throw new BusinessException("Client already has an active contract offer or active subscription for this product.");

        var basePrice = product.LicensePrice + dto.AdditionalSupportYears * 1000m;
        var discount = await _pricing.DiscountPercentAsync(dto.ClientId, OfferType.Contract, dto.StartDate);
        var finalPrice = Math.Round(basePrice * (1m - discount / 100m), 2);

        var contract = new Contract
        {
            ClientId = client.Id,
            SoftwareProductId = product.Id,
            SoftwareVersion = product.CurrentVersion,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AdditionalSupportYears = dto.AdditionalSupportYears,
            BasePrice = basePrice,
            DiscountPercentage = discount,
            FinalPrice = finalPrice
        };
        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync();
        return ToDto(contract);
    }

    public async Task<ContractDto> PayAsync(int contractId, PayContractDto dto)
    {
        if (dto.Amount <= 0) throw new BusinessException("Payment amount must be positive.");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contract = await _db.Contracts.Include(c => c.Payments).SingleOrDefaultAsync(c => c.Id == contractId)
            ?? throw new BusinessException("Contract not found.");
        if (contract.Status != ContractStatus.Offer) throw new BusinessException("Contract is not payable.");
        if (today > contract.EndDate)
        {
            contract.Status = ContractStatus.Expired;
            await _db.SaveChangesAsync();
            throw new BusinessException("Cannot accept payment after contract end date. Create a new offer.");
        }
        var paid = contract.Payments.Sum(p => p.Amount);
        if (paid + dto.Amount > contract.FinalPrice) throw new BusinessException("Payment would exceed contract price.");

        contract.Payments.Add(new ContractPayment { Amount = dto.Amount });
        if (paid + dto.Amount == contract.FinalPrice)
        {
            contract.Status = ContractStatus.Paid;
            contract.SignedAtUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return ToDto(contract);
    }

    public async Task DeleteAsync(int contractId)
    {
        var contract = await _db.Contracts.FindAsync(contractId) ?? throw new BusinessException("Contract not found.");
        contract.IsDeleted = true;
        await _db.SaveChangesAsync();
    }

    private static ContractDto ToDto(Contract c)
    {
        var paidAmount = c.Payments?.Sum(p => p.Amount) ?? 0m;
        var remainingAmount = c.FinalPrice - paidAmount;

        return new ContractDto(
            c.Id,
            c.ClientId,
            c.SoftwareProductId,
            c.SoftwareVersion,
            c.StartDate,
            c.EndDate,
            c.BasePrice,
            c.DiscountPercentage,
            c.FinalPrice,
            paidAmount,
            remainingAmount,
            c.Status.ToString()
        );
    }
}
