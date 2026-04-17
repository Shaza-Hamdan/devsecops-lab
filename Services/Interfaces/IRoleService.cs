using Registration.Persistence.entity;

public interface IRoleService
{
    Task<RoleEntity> GetRoleByNameAsync(string roleName); // Fetch role by its name
}