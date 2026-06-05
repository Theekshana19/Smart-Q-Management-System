using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IConfigurationService _config;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService admin, IConfigurationService config, ILogger<AdminController> logger)
    {
        _admin = admin;
        _config = config;
        _logger = logger;
    }

    [HttpGet("profile")]
    public Task<IActionResult> Profile(CancellationToken ct) =>
        Execute(() => _admin.GetProfileAsync(ct));

    [HttpGet("dashboard/summary")]
    public Task<IActionResult> DashboardSummary(CancellationToken ct) =>
        Execute(() => _admin.GetDashboardSummaryAsync(ct));

    [HttpGet("services/summary")]
    public Task<IActionResult> ServiceSummary(CancellationToken ct) =>
        Execute(() => _admin.GetServiceManagementSummaryAsync(ct));

    [HttpGet("services")]
    public Task<IActionResult> GetServices(
        [FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        Execute(() => _admin.GetServicesAsync(new ServiceListQuery(search, isActive, page, pageSize), ct));

    [HttpPost("services")]
    public Task<IActionResult> CreateService([FromBody] UpsertServiceRequest request, CancellationToken ct) =>
        Execute(() => _admin.CreateServiceAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("services/{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpsertServiceRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.UpdateServiceAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPatch("services/{id:int}/status")]
    public async Task<IActionResult> PatchServiceStatus(int id, [FromBody] PatchStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.PatchServiceStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("services/{id:int}")]
    public async Task<IActionResult> DeleteService(int id, CancellationToken ct)
    {
        var result = await Safe(() => _admin.DeleteServiceAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? NoContent() : NotFound();
    }

    [HttpGet("sub-services")]
    public Task<IActionResult> GetSubServices(
        [FromQuery] int? serviceId, [FromQuery] string? search, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        Execute(() => _admin.GetSubServicesAsync(new SubServiceListQuery(serviceId, search, isActive, page, pageSize), ct));

    [HttpPost("sub-services")]
    public Task<IActionResult> CreateSubService([FromBody] UpsertSubServiceRequest request, CancellationToken ct) =>
        Execute(() => _admin.CreateSubServiceAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("sub-services/{id:int}")]
    public async Task<IActionResult> UpdateSubService(int id, [FromBody] UpsertSubServiceRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.UpdateSubServiceAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPatch("sub-services/{id:int}/status")]
    public async Task<IActionResult> PatchSubServiceStatus(int id, [FromBody] PatchStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.PatchSubServiceStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("sub-services/{id:int}")]
    public async Task<IActionResult> DeleteSubService(int id, CancellationToken ct)
    {
        var result = await Safe(() => _admin.DeleteSubServiceAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? NoContent() : NotFound();
    }

    [HttpGet("counters")]
    public Task<IActionResult> GetCounters(CancellationToken ct) =>
        Execute(() => _admin.GetCountersAsync(ct));

    [HttpGet("counters/management")]
    public Task<IActionResult> GetCounterManagement(CancellationToken ct) =>
        Execute(() => _admin.GetCounterManagementAsync(ct));

    [HttpGet("counters/resource-load")]
    public Task<IActionResult> GetCounterResourceLoad(CancellationToken ct) =>
        Execute(() => _admin.GetCounterResourceLoadAsync(ct));

    [HttpPost("counters")]
    public Task<IActionResult> CreateCounter([FromBody] UpsertCounterRequest request, CancellationToken ct) =>
        Execute(() => _admin.CreateCounterAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("counters/{id:int}")]
    public async Task<IActionResult> UpdateCounter(int id, [FromBody] UpsertCounterRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.UpdateCounterAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPatch("counters/{id:int}/status")]
    public async Task<IActionResult> PatchCounterStatus(int id, [FromBody] PatchCounterStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.PatchCounterStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("counters/{id:int}")]
    public async Task<IActionResult> DeleteCounter(int id, CancellationToken ct)
    {
        var result = await Safe(() => _admin.DeleteCounterAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? NoContent() : NotFound();
    }

    [HttpGet("counter-assignments")]
    public Task<IActionResult> GetCounterAssignments(CancellationToken ct) =>
        Execute(() => _admin.GetCounterAssignmentsAsync(ct));

    [HttpGet("counters/{id:int}/assignable-services")]
    public Task<IActionResult> GetAssignableServices(int id, CancellationToken ct) =>
        Execute(() => _admin.GetAssignableServicesAsync(id, ct));

    [HttpPost("counter-assignments")]
    public Task<IActionResult> SaveCounterAssignments([FromBody] SaveCounterAssignmentRequest request, CancellationToken ct) =>
        Execute(async () => { await _admin.SaveCounterAssignmentsAsync(request, ct); return true; });

    [HttpGet("languages/summary")]
    public Task<IActionResult> LanguageSummary(CancellationToken ct) =>
        Execute(() => _config.GetLanguageManagementSummaryAsync(ct));

    [HttpGet("languages")]
    public Task<IActionResult> GetLanguages(CancellationToken ct) =>
        Execute(() => _config.GetLanguagesAsync(ct));

    [HttpPost("languages")]
    public Task<IActionResult> CreateLanguage([FromBody] UpsertLanguageRequest request, CancellationToken ct) =>
        Execute(() => _config.CreateLanguageAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("languages/{id:int}")]
    public async Task<IActionResult> UpdateLanguage(int id, [FromBody] UpsertLanguageRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.UpdateLanguageAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPatch("languages/{id:int}/status")]
    public async Task<IActionResult> PatchLanguageStatus(int id, [FromBody] PatchStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.PatchLanguageStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("languages/{id:int}")]
    public async Task<IActionResult> DeleteLanguage(int id, CancellationToken ct)
    {
        var result = await Safe(() => _config.DeleteLanguageAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? NoContent() : NotFound();
    }

    [HttpGet("settings")]
    public Task<IActionResult> GetSettings([FromQuery] string? search, [FromQuery] string? dataType, CancellationToken ct) =>
        Execute(() => _config.GetAdminSettingsAsync(new SettingListQuery(search, dataType), ct));

    [HttpGet("settings/{id:int}")]
    public async Task<IActionResult> GetSettingById(int id, CancellationToken ct)
    {
        var result = await Safe(() => _config.GetSettingByIdAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPut("settings/{id:int}")]
    public async Task<IActionResult> UpdateSetting(int id, [FromBody] UpdateAdminSettingRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.UpdateAdminSettingAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpGet("display-messages")]
    public Task<IActionResult> GetDisplayMessages(
        [FromQuery] int? languageId, [FromQuery] string? messageKey, [FromQuery] bool? isActive, CancellationToken ct) =>
        Execute(() => _config.GetDisplayMessagesAsync(new DisplayMessageListQuery(languageId, messageKey, isActive), ct));

    [HttpPost("display-messages")]
    public Task<IActionResult> CreateDisplayMessage([FromBody] UpsertDisplayMessageRequest request, CancellationToken ct) =>
        Execute(() => _config.CreateDisplayMessageAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("display-messages/{id:int}")]
    public async Task<IActionResult> UpdateDisplayMessage(int id, [FromBody] UpsertDisplayMessageRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.UpdateDisplayMessageAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPatch("display-messages/{id:int}/status")]
    public async Task<IActionResult> PatchDisplayMessageStatus(int id, [FromBody] PatchStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.PatchDisplayMessageStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("display-messages/{id:int}")]
    public async Task<IActionResult> DeleteDisplayMessage(int id, CancellationToken ct)
    {
        var result = await Safe(() => _config.DeleteDisplayMessageAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? NoContent() : NotFound();
    }

    [HttpGet("voice-templates")]
    public Task<IActionResult> GetVoiceTemplates(
        [FromQuery] int? languageId, [FromQuery] string? eventType, [FromQuery] bool? isActive, CancellationToken ct) =>
        Execute(() => _config.GetVoiceTemplatesAsync(new VoiceTemplateListQuery(languageId, eventType, isActive), ct));

    [HttpPost("voice-templates")]
    public Task<IActionResult> CreateVoiceTemplate([FromBody] UpsertVoiceTemplateRequest request, CancellationToken ct) =>
        Execute(() => _config.CreateVoiceTemplateAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("voice-templates/{id:int}")]
    public async Task<IActionResult> UpdateVoiceTemplate(int id, [FromBody] UpsertVoiceTemplateRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.UpdateVoiceTemplateAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPatch("voice-templates/{id:int}/status")]
    public async Task<IActionResult> PatchVoiceTemplateStatus(int id, [FromBody] PatchStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _config.PatchVoiceTemplateStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpDelete("voice-templates/{id:int}")]
    public async Task<IActionResult> DeleteVoiceTemplate(int id, CancellationToken ct)
    {
        var result = await Safe(() => _config.DeleteVoiceTemplateAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? NoContent() : NotFound();
    }

    [HttpGet("services/{serviceId:int}/translations")]
    public Task<IActionResult> GetServiceTranslations(int serviceId, CancellationToken ct) =>
        Execute(() => _config.GetServiceTranslationsAsync(serviceId, ct));

    [HttpPut("services/{serviceId:int}/translations")]
    public Task<IActionResult> SaveServiceTranslations(int serviceId, [FromBody] IReadOnlyList<UpsertServiceTranslationRequest> items, CancellationToken ct) =>
        Execute(async () => { await _config.SaveServiceTranslationsAsync(serviceId, items, ct); return true; });

    [HttpGet("sub-services/{subServiceId:int}/translations")]
    public Task<IActionResult> GetSubServiceTranslations(int subServiceId, CancellationToken ct) =>
        Execute(() => _config.GetSubServiceTranslationsAsync(subServiceId, ct));

    [HttpPut("sub-services/{subServiceId:int}/translations")]
    public Task<IActionResult> SaveSubServiceTranslations(int subServiceId, [FromBody] IReadOnlyList<UpsertSubServiceTranslationRequest> items, CancellationToken ct) =>
        Execute(async () => { await _config.SaveSubServiceTranslationsAsync(subServiceId, items, ct); return true; });

    [HttpGet("reports/token-history")]
    public Task<IActionResult> TokenHistory([FromQuery] TokenHistoryFilter filter, CancellationToken ct) =>
        Execute(() => _admin.GetTokenHistoryReportAsync(filter, ct));

    [HttpGet("staff/summary")]
    public Task<IActionResult> StaffSummary(CancellationToken ct) =>
        Execute(() => _admin.GetStaffManagementSummaryAsync(ct));

    [HttpGet("staff")]
    public Task<IActionResult> GetStaff(
        [FromQuery] string? search, [FromQuery] string? role, [FromQuery] bool? isActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default) =>
        Execute(() => _admin.GetStaffAsync(new StaffListQuery(search, role, isActive, page, pageSize), ct));

    [HttpPost("staff")]
    public Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request, CancellationToken ct) =>
        Execute(() => _admin.CreateStaffAsync(request, ct), StatusCodes.Status201Created);

    [HttpPut("staff/{id:int}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.UpdateStaffAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPost("staff/{id:int}/reset-password")]
    public async Task<IActionResult> ResetStaffPassword(int id, [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.ResetStaffPasswordAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? Ok(new { success = true }) : NotFound();
    }

    [HttpPatch("staff/{id:int}/status")]
    public async Task<IActionResult> PatchStaffStatus(int id, [FromBody] PatchStaffStatusRequest request, CancellationToken ct)
    {
        var result = await Safe(() => _admin.PatchStaffStatusAsync(id, request, ct));
        if (result.Error != null) return result.Error;
        return result.Value == null ? NotFound() : Ok(result.Value);
    }

    [HttpPost("staff/{id:int}/force-logout")]
    public async Task<IActionResult> ForceLogoutStaff(int id, CancellationToken ct)
    {
        var result = await Safe(() => _admin.ForceLogoutStaffAsync(id, ct));
        if (result.Error != null) return result.Error;
        return result.Value ? Ok(new { success = true }) : NotFound();
    }

    private async Task<IActionResult> Execute<T>(Func<Task<T>> action, int successStatus = StatusCodes.Status200OK)
    {
        var result = await Safe(action);
        if (result.Error != null) return result.Error;
        return successStatus == StatusCodes.Status201Created
            ? Created(string.Empty, result.Value)
            : Ok(result.Value);
    }

    private async Task<(T? Value, IActionResult? Error)> Safe<T>(Func<Task<T>> action)
    {
        try
        {
            return (await action(), null);
        }
        catch (InvalidOperationException ex)
        {
            return (default, BadRequest(new { message = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin API error");
            return (default, StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message }));
        }
    }
}
