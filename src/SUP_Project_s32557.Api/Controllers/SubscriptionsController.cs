using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SUP_Project_s32557.Api.Dtos;
using SUP_Project_s32557.Api.Services;

namespace SUP_Project_s32557.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _service;
    public SubscriptionsController(ISubscriptionService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Buy(BuySubscriptionDto dto) => Created("", await _service.BuyAsync(dto));

    [HttpPost("{id:int}/renewal-payments")]
    public async Task<IActionResult> Renew(int id, PaySubscriptionDto dto) => Ok(await _service.RenewAsync(id, dto));
}
