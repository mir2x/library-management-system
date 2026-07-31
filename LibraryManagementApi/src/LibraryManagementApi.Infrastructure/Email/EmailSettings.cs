namespace LibraryManagementApi.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public required string SmtpHost { get; set; }

    public int SmtpPort { get; set; } = 587;

    public required string SenderEmail { get; set; }

    public string SenderName { get; set; } = "Library Management System";

    public required string Username { get; set; }

    public required string Password { get; set; }
}
