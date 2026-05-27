namespace LMS.Notification.API.Application.DTOs;

public sealed record SchoolNoticeDto(Guid Id, string Title, string Date, string Priority);

public sealed record NotificationItemDto(string Title, string Desc, string Time);
