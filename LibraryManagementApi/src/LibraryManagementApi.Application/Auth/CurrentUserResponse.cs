namespace LibraryManagementApi.Application.Auth;

public record CurrentUserResponse(string UserId, string Email, string FullName, IReadOnlyList<string> Roles);
