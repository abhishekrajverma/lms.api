namespace LMS.Identity.API.Application.DTOs;

public sealed record UserProfileDto(string UserId, string Email, string? DisplayName, IReadOnlyList<string> Roles);
