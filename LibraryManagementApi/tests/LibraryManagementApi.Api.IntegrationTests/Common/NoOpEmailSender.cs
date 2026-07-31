using System.Collections.Concurrent;
using LibraryManagementApi.Application.Common.Interfaces;

namespace LibraryManagementApi.Api.IntegrationTests.Common;

public record SentEmail(string ToEmail, string Subject, string HtmlBody);

// Real SMTP delivery is neither desirable nor reachable in CI/test runs; this captures what
// would have been sent so tests (e.g. the password reset flow) can inspect it instead.
public class NoOpEmailSender : IEmailSender
{
    private readonly ConcurrentBag<SentEmail> _sentEmails = [];

    public IReadOnlyCollection<SentEmail> SentEmails => _sentEmails;

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        _sentEmails.Add(new SentEmail(toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}
