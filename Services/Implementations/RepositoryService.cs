using Registration.Persistence.Repository;
using Services.Interfaces;
using Persistence.entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using GitFile.DTO;

namespace Services.Implementations
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDBContext _db;
        private readonly IGitService _git;

        public RepositoryService(IWebHostEnvironment env, AppDBContext db, IGitService git)
        {
            _env = env;
            _db = db;
            _git = git;
        }

        public Guid CreateRepository(Guid userId, string repositoryName)
        {
            var repositoryId = Guid.NewGuid();

            var repoPath = Path.Combine(
                _env.ContentRootPath,
                "AppData",
                "Repositories",
                repositoryId.ToString()
            );

            Directory.CreateDirectory(repoPath);

            // Initialize Git
            _git.InitializeRepository(repoPath);

            //create README
            var readmePath = Path.Combine(repoPath, "README.md");
            File.WriteAllText(readmePath, $"# {repositoryName}");

            _git.CommitChanges(repoPath, "Initial commit", "System", "system@gitfile.app");

            // Save repository in DB
            var repository = new TheRepository
            {
                Id = repositoryId,
                Name = repositoryName,
                OwnerId = userId,
                Path = repoPath,
                CreatedAt = DateTime.UtcNow
            };

            _db.therepository.Add(repository);

            // Add owner as collaborator
            _db.repositoryCollaborator.Add(new RepositoryCollaborator
            {
                Id = Guid.NewGuid(),
                RepositoryId = repositoryId,
                UserId = userId,
                Role = RepositoryRole.Owner
            });

            _db.SaveChanges();

            return repositoryId;
        }

        public void AddCollaborator(Guid repositoryId, Guid ownerId, Guid collaboratorId, RepositoryRole role)
        {
            var repo = _db.therepository
                .Include(r => r.Collaborators)
                .FirstOrDefault(r => r.Id == repositoryId);

            if (repo == null)
                throw new Exception("Repository not found.");

            // Security check
            if (repo.OwnerId != ownerId)
                throw new UnauthorizedAccessException("Only the owner can add collaborators.");

            // Check collaborator exists
            var collaboratorExists = _db.registrations
                .Any(u => u.Id == collaboratorId);

            if (!collaboratorExists)
                throw new Exception("User does not exist.");

            // Check if already collaborator
            var alreadyExists = repo.Collaborators
                .Any(c => c.UserId == collaboratorId);

            if (alreadyExists)
                throw new Exception("User is already a collaborator.");

            var newCollab = new RepositoryCollaborator
            {
                Id = Guid.NewGuid(),
                RepositoryId = repositoryId,
                UserId = collaboratorId,
                Role = role
            };

            _db.repositoryCollaborator.Add(newCollab);
            _db.SaveChanges();
        }


    }


}