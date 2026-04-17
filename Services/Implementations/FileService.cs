using GitFile.DTO;
using GitFile.FileCreate;
using LibGit2Sharp;
using Microsoft.EntityFrameworkCore;
using Persistence.entity;
using Registration.Persistence.entity;
using Registration.Persistence.Repository;
public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly AppDBContext appdbContext;
    private readonly IGitService _git;

    public FileService(IWebHostEnvironment env, AppDBContext appDbContext, IGitService git)
    {
        _env = env;
        appdbContext = appDbContext;
        _git = git;
    }

    public Guid CreateFile(Guid repoId, Guid userId, string fileName, string content)
    {
        var repo = appdbContext.therepository.Include(r => r.Collaborators)
            .FirstOrDefault(r => r.Id == repoId);

        if (repo == null)
            throw new Exception("Repository not found.");

        // Check if user is owner
        var isOwner = repo.OwnerId == userId;
        if (!isOwner) throw new UnauthorizedAccessException("Only owner can add files.");

        var user = appdbContext.registrations.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            throw new Exception("User not found.");

        var fileId = Guid.NewGuid();

        var basePath = Path.GetFullPath(repo.Path);
        Directory.CreateDirectory(basePath);

        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(basePath, safeFileName);

        if (!filePath.StartsWith(basePath))
            throw new UnauthorizedAccessException("Invalid file path");

        File.WriteAllText(filePath, content);

        _git.CommitChanges(repo.Path, $"Add file {fileName}", user.UserName, user.Email);
        var file = new RepositoryFile
        {
            Id = fileId,
            RepositoryId = repoId,
            FileName = fileName,
            Path = filePath,
            CreatedAt = DateTime.UtcNow
        };

        appdbContext.repositoryFile.Add(file);
        appdbContext.SaveChanges();

        // Optionally: Commit in Git repo here for versioning
        return fileId;
    }

    public void EditFile(string repoPath, string branchName, string filePath, string newContent, string userName, string userEmail)
    {
        using var repo = new Repository(repoPath);

        var branch = repo.Branches[branchName]; //Looks up the branch

        if (branch == null)
            throw new Exception("Branch not found.");

        Commands.Checkout(repo, branch);// switch to the branch

        File.WriteAllText(filePath, newContent);

        Commands.Stage(repo, "*");//Stages all changes in the repository (equivalent to git add .).

        var signature = new Signature(userName, userEmail, DateTimeOffset.UtcNow);

        repo.Commit("Edit file", signature, signature);
    }
}
