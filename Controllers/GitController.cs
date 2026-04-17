using System.Security.Claims;
using GitFile.Extensions;
using LibGit2Sharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Registration.Persistence.Repository;
using Services.Interfaces;

[ApiController]
[Route("api/repositories/{repositoryId}/git")]
[Authorize]
public class RepositoryGitController : ControllerBase
{
    private readonly IRepositoryService _repositoryService;
    private readonly IGitService _gitService;
    private readonly AppDBContext _db;

    public RepositoryGitController(AppDBContext db, IRepositoryService repositoryService, IGitService gitService)
    {
        _repositoryService = repositoryService;
        _gitService = gitService;
        _db = db;
    }

    [HttpGet("{repoId}/files/{fileId}/commits")]
    public IActionResult GetFileCommits([FromRoute] Guid repoId, [FromRoute] Guid fileId)
    {
        var repo = _db.therepository.FirstOrDefault(r => r.Id == repoId);
        var file = _db.repositoryFile.FirstOrDefault(f => f.Id == fileId);

        if (repo == null || file == null)
            return NotFound("Repository or file not found");

        var commits = _gitService.GetFileCommits(repo.Path, file.Path);

        return Ok(commits);
    }
    [HttpGet("{repoId}/check-merge")]
    public IActionResult CheckMerge(Guid repoId, string sourceBranch)
    {
        var repo = _db.therepository.FirstOrDefault(r => r.Id == repoId);

        if (repo == null)
            return NotFound();

        var result = _gitService.CheckMergeConflicts(repo.Path, sourceBranch);

        return Ok(result);
    }

    [HttpGet("{repoId}/merge-diff")]
    public IActionResult GetMergeDiff(Guid repoId, string sourceBranch)
    {
        var repo = _db.therepository.FirstOrDefault(r => r.Id == repoId);

        if (repo == null)
            return NotFound();

        var diff = _gitService.GetBranchDiff(repo.Path, sourceBranch);

        return Ok(diff);
    }
    // [HttpGet("{repoId}/files/{fileId}/diff/{commitId}")]
    // public IActionResult GetFileDiff(Guid repoId, Guid fileId, string commitId)
    // {
    //     var repo = _db.therepository.FirstOrDefault(r => r.Id == repoId);
    //     var file = _db.repositoryFile.FirstOrDefault(f => f.Id == fileId);

    //     if (repo == null || file == null)
    //         return NotFound("Repository or file not found");

    //     var diff = _gitService.GetFileDiff(repo.Path, commitId, file.Path);

    //     return Ok(diff);
    // }
    // [HttpPost("commit")]
    // public IActionResult Commit(Guid repositoryId, string message)
    // {
    //     var userId = User.GetUserId();

    //     var repository = _repositoryService.ValidateAccess(repositoryId, userId);

    //     var userName = User.Identity?.Name;
    //     var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

    //     _gitService.CommitChanges(repository.RepositoryPath, message, userName, userEmail);

    //     return Ok("Commit successful.");
    // }

    // [HttpPost("branches")]
    // public IActionResult CreateBranch(Guid repositoryId, string branchName)
    // {
    //     var userId = User.GetUserId();

    //     var repository = _repositoryService.ValidateAccess(repositoryId, userId);

    //     _gitService.CreateBranch(repository.RepositoryPath, branchName);

    //     return Ok("Branch created.");
    // }

    // [HttpPost("merge")]
    // public IActionResult Merge(Guid repositoryId, string sourceBranch)
    // {
    //     var userId = User.GetUserId();

    //     var repository = _repositoryService.ValidateAccess(repositoryId, userId);

    //     var result = _gitService.MergeBranch(repository.RepositoryPath, sourceBranch);

    //     return Ok(result);
    // }
}