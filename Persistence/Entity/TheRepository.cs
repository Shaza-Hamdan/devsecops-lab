namespace Persistence.entity
{
    public class TheRepository
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid OwnerId { get; set; }

        public string Path { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<RepositoryCollaborator> Collaborators { get; set; }
    }
}