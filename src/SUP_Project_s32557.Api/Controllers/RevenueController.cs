using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SUP_Project_s32557.Api.Dtos;
using SUP_Project_s32557.Api.Services;

namespace SUP_Project_s32557.Api.Controllers;

[ApiController]
[Route("api/revenue")]
[Authorize]
public class RevenueController : ControllerBase
{
    private readonly IRevenueService _revenue;
    private readonly ICurrencyService _currency;
    public RevenueController(IRevenueService revenue, ICurrencyService currency) { _revenue = revenue; _currency = currency; }

    [HttpGet("current")]
    public async Task<IActionResult> Current([FromQuery] int? productId, [FromQuery] string currency = "PLN")
    {
        var pln = await _revenue.CurrentRevenuePlnAsync(productId);
        return Ok(new RevenueDto(_currency.ConvertFromPln(pln, currency), currency.ToUpperInvariant()));
    }

    [HttpGet("predicted")]
    public async Task<IActionResult> Predicted([FromQuery] int? productId, [FromQuery] int monthsAhead = 12, [FromQuery] string currency = "PLN")
    {
        var pln = await _revenue.PredictedRevenuePlnAsync(productId, monthsAhead);
        return Ok(new RevenueDto(_currency.ConvertFromPln(pln, currency), currency.ToUpperInvariant()));
    }
}
