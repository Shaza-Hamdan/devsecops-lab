using GitFile.DTO;
using Registration.Persistence.entity;

public interface IAdminService
{
    Task<List<RegistrationWithRoleDto>> GetAllRegistrationsAsync();
    Task<RegistrationUser> GetRegistrationByIdAsync(Guid id);
    Task UpdateRegistrationAsync(RegistrationUser registration);
    Task<bool> DeleteRegistrationAsync(Guid id);
}
