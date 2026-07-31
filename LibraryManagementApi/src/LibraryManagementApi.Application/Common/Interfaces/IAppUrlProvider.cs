namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IAppUrlProvider
{
    string BuildPasswordResetUrl(string email, string token);
}
