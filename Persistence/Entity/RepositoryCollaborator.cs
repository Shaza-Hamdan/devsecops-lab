
using System.Text.Json.Serialization;

namespace Persistence.entity
{
    public class RepositoryCollaborator
    {
        public Guid Id { get; set; }

        public Guid RepositoryId { get; set; }

        public Guid UserId { get; set; }

        public RepositoryRole Role { get; set; }

        [JsonIgnore]
        public TheRepository Repository { get; set; }
    }
}