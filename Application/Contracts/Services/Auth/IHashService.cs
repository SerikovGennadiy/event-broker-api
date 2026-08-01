namespace Application.Contracts.Services.Auth;

internal interface IHashService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
