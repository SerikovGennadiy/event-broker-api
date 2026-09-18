namespace Users.Application.Contracts.Services;

public interface IHashService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
