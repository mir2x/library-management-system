namespace LibraryManagementApi.Application.Common.Models;

public record AuthenticatedUser(string Id, string Email, string FullName, IReadOnlyList<string> Roles);
