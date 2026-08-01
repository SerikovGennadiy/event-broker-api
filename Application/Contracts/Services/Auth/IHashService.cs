namespace Application.Contracts.Services.Auth;

public interface IHashService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
