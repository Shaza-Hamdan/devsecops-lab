namespace Registration.Persistence.entity
{
    public class UserFile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; }
        public string RepositoryPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}