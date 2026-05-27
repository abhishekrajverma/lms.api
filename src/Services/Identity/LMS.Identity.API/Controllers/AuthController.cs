using System.Security.Claims;
using LMS.Identity.API.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "unknown";
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? string.Empty;
        var displayName = User.FindFirstValue("name");
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new UserProfileDto(userId, email, displayName, roles));
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { service = "LMS.Identity.API" });
}
