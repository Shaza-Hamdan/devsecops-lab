namespace Registration.Persistence.entity
{
    public class RoleEntity
    {
        public Guid Id { get; set; } // Primary Key
        public string Name { get; set; } // Role Name (e.g., Admin, User, Guest)

        // Navigation property for associated users
        public ICollection<RegistrationUser> Registrations { get; set; }
    }
}