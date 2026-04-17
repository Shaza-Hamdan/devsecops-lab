using GitFile.DTO;
using Registration.Persistence.entity;
using Registration.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

public class AdminService : IAdminService
{
    private readonly AppDBContext _context;

    public AdminService(AppDBContext context)
    {
        _context = context;
    }

    // Fetch all registrations with the info
    public async Task<List<RegistrationWithRoleDto>> GetAllRegistrationsAsync()
    {
        return await _context.registrations
          .Select(r => new RegistrationWithRoleDto(
              r.Id,
              r.UserName,
              r.Email,
              r.PhoneNumber,
              r.role.Name // Fetch only the Role name
          ))
          .ToListAsync();
    }

    public async Task<RegistrationUser> GetRegistrationByIdAsync(Guid id)
    {
        var registration = await _context.registrations
                                        .FirstOrDefaultAsync(r => r.Id == id);
        if (registration == null)
        {
            throw new Exception($"Registration with ID {id} not found.");
        }
        return registration;
    }


    public async Task UpdateRegistrationAsync(RegistrationUser registration)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration), "Registration cannot be null.");
        }

        _context.registrations.Update(registration);
        await _context.SaveChangesAsync();
    }


    public async Task<bool> DeleteRegistrationAsync(Guid id)
    {
        var registration = await _context.registrations.FindAsync(id);
        if (registration == null)
        {
            return false; // User not found
        }

        _context.registrations.Remove(registration);
        await _context.SaveChangesAsync();
        return true;
    }

}
