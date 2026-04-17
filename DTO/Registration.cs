
namespace GitFile.DTO
{
    public record CreateNewAccountRequest(
        string UserName,
        string Password,
        string Email,
        string PhoneNumber
    );

    public record LoginRequest(
        string Email,
        string Password
    );

}
