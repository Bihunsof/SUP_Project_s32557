using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SUP_Project_s32557.Api.Dtos;
using SUP_Project_s32557.Api.Services;

namespace SUP_Project_s32557.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;
    public ClientsController(IClientService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.GetAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateClientDto dto) => Created("", await _service.CreateAsync(dto));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateClientDto dto) => Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) { await _service.DeleteAsync(id); return NoContent(); }
}
