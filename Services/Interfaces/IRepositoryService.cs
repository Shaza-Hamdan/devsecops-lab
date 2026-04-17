using GitFile.DTO;
using Persistence.entity;

namespace Services.Interfaces
{
    public interface IRepositoryService
    {
        Guid CreateRepository(Guid userId, string repositoryName);
        public void AddCollaborator(Guid repositoryId, Guid ownerId, Guid collaboratorId, RepositoryRole role);
    }
}