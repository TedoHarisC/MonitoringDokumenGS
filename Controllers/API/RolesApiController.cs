using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "SUPER_ADMIN")]
[ApiController]
[Route("api/roles")]
public class RolesApiController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesApiController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllRolesAsync());
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(Guid userId, int roleId)
    {
        await _service.AssignRoleAsync(userId, roleId);
        return Ok();
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(Guid userId, int roleId)
    {
        await _service.RemoveRoleAsync(userId, roleId);
        return Ok();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        return Ok(await _service.GetUserRolesAsync(userId));
    }
}
