using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageService _service;

    public LanguagesController(ILanguageService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await _service.GetActiveLanguagesAsync(ct));
}
