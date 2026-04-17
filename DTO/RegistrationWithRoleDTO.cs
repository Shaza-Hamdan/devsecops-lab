namespace GitFile.DTO
{
    public record RegistrationWithRoleDto
    (
          Guid Id,
          string UserName,
          string Email,
          string PhoneNumber,
          string RoleName // To hold the Role.Name
    );
}