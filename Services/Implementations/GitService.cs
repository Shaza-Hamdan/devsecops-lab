using GitFile.DTO;
using LibGit2Sharp;

public class GitService : IGitService
{
    public void InitializeRepository(string repositoryPath)
    {
        if (!Repository.IsValid(repositoryPath))
        {
            Repository.Init(repositoryPath);
            using var repo = new Repository(repositoryPath);

            // Get the current branch created by git init
            var currentBranch = repo.Head;

            // If it's not "main", rename it
            if (currentBranch.FriendlyName != "main")
            {
                repo.Branches.Rename(currentBranch, "main");
            }
            Console.WriteLine($"Repository initialized at {repositoryPath}");
        }
        else
        {
            Console.WriteLine($"Repository already exists at {repositoryPath}");
        }
    }
    public void CommitChanges(string repositoryPath, string commitMessage, string userName, string userEmail)
    {
        //open the repository
        using var repo = new Repository(repositoryPath);

        // Stage all changes (add/edit/delete)
        Commands.Stage(repo, "*");

        // Prepare signature
        var signature = new Signature(userName, userEmail, DateTimeOffset.UtcNow);

        // Commit
        repo.Commit(commitMessage, signature, signature);
    }

    public void CreateBranch(string repoPath, string branchName)
    {
        using var repo = new Repository(repoPath);

        var mainBranch = repo.Branches["main"];

        if (mainBranch == null)
            throw new Exception("Main branch not found.");

        if (repo.Branches[branchName] != null)
            throw new Exception("Branch already exists.");

        var newBranch = repo.CreateBranch(branchName, mainBranch.Tip);
        Commands.Checkout(repo, newBranch);
    }
    public string MergeBranch(string repoPath, string sourceBranch, string userName, string userEmail)
    {
        using var repo = new Repository(repoPath);

        // Get branches
        var source = repo.Branches[sourceBranch];
        var target = repo.Branches["main"]; // always merge into main

        if (source == null)
            throw new Exception("Source branch not found.");

        if (target == null)
            throw new Exception("Target branch (main) not found.");

        // Checkout main before merging
        Commands.Checkout(repo, target);

        // Create signature (REAL USER, not System)
        var signature = new Signature(userName, userEmail, DateTimeOffset.UtcNow);

        // Perform merge
        var result = repo.Merge(source, signature);

        // Handle result
        if (result.Status == MergeStatus.Conflicts)
        {
            return "Merge failed: conflicts detected.";
        }

        if (result.Status == MergeStatus.UpToDate)
        {
            return "Already up to date.";
        }

        if (result.Status == MergeStatus.FastForward)
        {
            return "Merge completed (fast-forward).";
        }

        if (result.Status == MergeStatus.NonFastForward)
        {
            return "Merge completed with commit.";
        }

        return "Merge completed.";
    }
    // public void SwitchToBranch(string repoPath, string branchName)
    // {
    //     using var repo = new Repository(repoPath);

    //     var branch = repo.Branches[branchName];
    //     if (branch == null)
    //         throw new Exception("Branch does not exist.");

    //     Commands.Checkout(repo, branch);
    // }

    public string CheckMergeConflicts(string repoPath, string sourceBranch, string targetBranch = "main")
    {
        using var repo = new Repository(repoPath);

        var source = repo.Branches[sourceBranch];
        var target = repo.Branches[targetBranch];

        if (source == null || target == null)
            throw new Exception("Branch not found");

        var result = repo.Merge(source, new Signature("Temp", "temp@test.com", DateTimeOffset.Now), new MergeOptions
        {
            FailOnConflict = true
        });

        if (result.Status == MergeStatus.Conflicts)
            return "Conflict";

        return "Clean";
    }

    public string GetBranchDiff(string repoPath, string sourceBranch, string targetBranch = "main")
    {
        using var repo = new Repository(repoPath);

        var source = repo.Branches[sourceBranch];
        var target = repo.Branches[targetBranch];

        if (source == null || target == null)
            throw new Exception("Branch not found");

        var patch = repo.Diff.Compare<Patch>(
            target.Tip.Tree,
            source.Tip.Tree
        );

        return patch.Content;
    }

    public IReadOnlyList<DiffEntry> GetDiff(string repositoryPath)
    {
        using var repo = new Repository(repositoryPath);

        var changes = repo.Diff.Compare<TreeChanges>(
            repo.Head.Tip.Tree,
            DiffTargets.WorkingDirectory
        );

        return changes
            .Select(c => new DiffEntry(
                c.Path,
                c.Status.ToString()
            ))
            .ToList();
    }

    public List<object> GetFileCommits(string repoPath, string filePath)
    {
        using var repo = new Repository(repoPath);
        var relativeFilePath = Path.GetRelativePath(repoPath, filePath).Replace("\\", "/");

        var commits = repo.Commits
            .QueryBy(relativeFilePath) //Give me only commits that changed this file (relativeFilePath).
            .Select(c => new
            {
                CommitId = c.Commit.Id.Sha,
                Message = c.Commit.MessageShort,
                Author = c.Commit.Author.Name,
                Email = c.Commit.Author.Email,
                Date = c.Commit.Author.When
            })
            .ToList<object>();

        return commits;
    }

    public List<ConflictResult> GetConflictDetails(string repoPath, string sourceBranch, string targetBranch = "main")
    {
        using var repo = new Repository(repoPath);

        var source = repo.Branches[sourceBranch];
        var target = repo.Branches[targetBranch];

        if (source == null || target == null)
            throw new Exception("Branch not found");

        var conflicts = new List<ConflictResult>();

        // Compare trees
        var changes = repo.Diff.Compare<TreeChanges>(
            target.Tip.Tree,
            source.Tip.Tree
        );

        foreach (var change in changes.Where(c => c.Status == ChangeKind.Modified))
        {
            var path = change.Path;

            // Get file content from MAIN
            var mainBlob = target.Tip[path]?.Target as Blob;
            var mainContent = mainBlob != null
                ? new StreamReader(mainBlob.GetContentStream()).ReadToEnd()
                : "";

            // Get file content from BRANCH
            var branchBlob = source.Tip[path]?.Target as Blob;
            var branchContent = branchBlob != null
                ? new StreamReader(branchBlob.GetContentStream()).ReadToEnd()
                : "";

            conflicts.Add(new ConflictResult(
                path,
                mainContent,
                branchContent
            ));
        }

        return conflicts;
    }
}
