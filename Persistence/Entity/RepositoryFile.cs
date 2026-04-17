namespace Persistence.entity
{
    public class RepositoryFile
    {
        public Guid Id { get; set; }

        public Guid RepositoryId { get; set; }

        public string FileName { get; set; }

        public DateTime CreatedAt { get; set; }
        public string Path { get; set; }
    }
}