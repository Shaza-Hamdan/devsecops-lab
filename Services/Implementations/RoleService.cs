using Registration.Persistence.entity;
using Registration.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

public class RoleService : IRoleService
{
    private readonly AppDBContext _context;

    public RoleService(AppDBContext context)
    {
        _context = context;
    }

    // Fetch role by its name
    public async Task<RoleEntity> GetRoleByNameAsync(string name)
    {
        var role = await _context.Roles
                                .FirstOrDefaultAsync(r => EF.Functions.Like(r.Name, $"%{name}%"));

        if (role == null)
        {
            throw new ArgumentException("Role not found.");
        }

        return role;
    }



}
