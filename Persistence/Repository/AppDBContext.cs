using Microsoft.EntityFrameworkCore;
using Registration.Persistence.entity;
using Persistence.entity;

namespace Registration.Persistence.Repository
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<RegistrationUser> registrations { get; set; }
        public DbSet<UserFile> userFiles { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<EmailVerification> emailVerification { get; set; }
        public DbSet<RepositoryCollaborator> repositoryCollaborator { get; set; }
        public DbSet<RepositoryFile> repositoryFile { get; set; }
        public DbSet<TheRepository> therepository { get; set; }
        public DbSet<MergeRequest> mergeRequest { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepositoryCollaborator>()
             .Property(r => r.Role)
             .HasConversion<string>();
            base.OnModelCreating(modelBuilder);

        }

    }
}