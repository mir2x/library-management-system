namespace LibraryManagementApi.Application.Members;

public static class MembershipNumberGenerator
{
    public static string Generate() => $"MEM-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
