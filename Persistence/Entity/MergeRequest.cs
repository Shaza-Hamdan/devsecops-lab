namespace Persistence.entity
{
    public class MergeRequest
    {
        public Guid Id { get; set; }
        public Guid RepositoryId { get; set; }
        public string BranchName { get; set; } // store branch name
        public Guid CreatedByUserId { get; set; } // collaborator
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsMerged { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? MergedAt { get; set; }
    }
}