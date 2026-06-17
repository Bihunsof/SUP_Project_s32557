using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SUP_Project_s32557.Api.Data;
using SUP_Project_s32557.Api.Dtos;

namespace SUP_Project_s32557.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetProducts() => Ok(await _db.SoftwareProducts
        .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.CurrentVersion, p.Category, p.LicensePrice)).ToListAsync());

    [HttpGet("subscription-plans")]
    public async Task<IActionResult> GetPlans() => Ok(await _db.SubscriptionPlans
        .Select(p => new SubscriptionPlanDto(p.Id, p.SoftwareProductId, p.Name, p.RenewalPeriodMonths, p.PricePerPeriod)).ToListAsync());
}
