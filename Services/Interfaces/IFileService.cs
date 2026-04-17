using GitFile.DTO;
namespace GitFile.FileCreate
{
    public interface IFileService
    {
        Guid CreateFile(Guid repoId, Guid userId, string fileName, string content);
        void EditFile(string repoPath, string branchName, string filePath, string newContent, string userName, string userEmail);
    }
}