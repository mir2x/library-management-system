namespace LibraryManagementApi.Infrastructure.Configuration;

public class ClientAppOptions
{
    public const string SectionName = "ClientApp";

    public required string BaseUrl { get; set; }

    public string PasswordResetPath { get; set; } = "/reset-password";
}
