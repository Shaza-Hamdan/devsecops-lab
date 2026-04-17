using GitFile.DTO;

namespace Registration.Services
{
    public interface IRegistrationService
    {
        Task Register(CreateNewAccountRequest account);
        string Login(LoginRequest account);

    }
}