using GitFile.DTO;

public interface IGitService
{
    void InitializeRepository(string repositoryPath);
    void CommitChanges(string repositoryPath, string commitMessage, string userName, string userEmail);
    void CreateBranch(string repositoryPath, string branchName);
    string CheckMergeConflicts(string repoPath, string sourceBranch, string targetBranch = "main");
    string GetBranchDiff(string repoPath, string sourceBranch, string targetBranch = "main");
    IReadOnlyList<DiffEntry> GetDiff(string repositoryPath);
    List<object> GetFileCommits(string repoPath, string relativeFilePath);
    string MergeBranch(string repoPath, string sourceBranch, string userName, string userEmail);
    List<ConflictResult> GetConflictDetails(string repoPath, string sourceBranch, string targetBranch = "main");
}
