using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SUP_Project_s32557.Api.Dtos;
using SUP_Project_s32557.Api.Services;

namespace SUP_Project_s32557.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _service;
    public ContractsController(IContractService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create(CreateContractDto dto) => Created("", await _service.CreateAsync(dto));

    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> Pay(int id, PayContractDto dto) => Ok(await _service.PayAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) { await _service.DeleteAsync(id); return NoContent(); }
}
