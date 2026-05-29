using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _catalog;

    public ServicesController(IServiceCatalogService catalog) => _catalog = catalog;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string languageCode = "EN", CancellationToken ct = default) =>
        Ok(await _catalog.GetServicesAsync(languageCode, ct));

    [HttpGet("{serviceId:int}/sub-services")]
    public async Task<IActionResult> GetSubServices(int serviceId, [FromQuery] string languageCode = "EN", CancellationToken ct = default) =>
        Ok(await _catalog.GetSubServicesAsync(serviceId, languageCode, ct));

    [HttpGet("kiosk-status")]
    public async Task<IActionResult> GetKioskStatus(CancellationToken ct = default) =>
        Ok(await _catalog.GetKioskStatusAsync(ct));
}
