using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;
using GitFile.Extensions;
using Persistence.entity;
using Registration.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using LibGit2Sharp;
using GitFile.DTO;
using Persistence.entity;

[ApiController]
[Route("api/[controller]")]
public class MergeRequestController : ControllerBase
{
    private readonly AppDBContext _db;
    private readonly IGitService _git;
    public MergeRequestController(AppDBContext db, IGitService git)
    {
        _git = git;
        _db = db;
    }

    [HttpPost("{repositoryId}/merge-request")]
    public IActionResult CreateMergeRequest(Guid repositoryId, string branchName, string title, string description)
    {
        var userId = User.GetUserId();

        var mergeRequest = new MergeRequest
        {
            Id = Guid.NewGuid(),
            RepositoryId = repositoryId,
            BranchName = branchName,
            CreatedByUserId = userId,
            Title = title,
            Description = description,
            IsMerged = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.mergeRequest.Add(mergeRequest);
        _db.SaveChanges();

        return Ok(new { Message = "Merge request created", MergeRequestId = mergeRequest.Id });
    }

    [HttpGet("{repositoryId}/merge-requests")]
    public IActionResult GetMergeRequests(Guid repositoryId)
    {
        var userId = User.GetUserId();
        var repository = _db.therepository.FirstOrDefault(r => r.Id == repositoryId);

        if (repository.OwnerId != userId)
            return Forbid("Only owner can view merge requests.");

        var requests = _db.mergeRequest
            .Where(mr => mr.RepositoryId == repositoryId && !mr.IsMerged)
            .ToList();

        return Ok(requests);
    }


    [HttpPost("{repositoryId}/merge-request/{mergeRequestId}/merge")]
    public IActionResult MergeRequest(Guid repositoryId, Guid mergeRequestId)
    {
        try
        {
            var userId = User.GetUserId();

            var repository = _db.therepository.FirstOrDefault(r => r.Id == repositoryId);
            if (repository == null)
                return NotFound("Repository not found.");

            if (repository.OwnerId != userId)
                return Forbid("Only owner can merge.");

            var mergeRequest = _db.mergeRequest
                .FirstOrDefault(mr => mr.Id == mergeRequestId && mr.RepositoryId == repositoryId);

            if (mergeRequest == null)
                return NotFound("Merge request not found.");

            if (mergeRequest.IsMerged)
                return BadRequest("Already merged.");

            var owner = _db.registrations.FirstOrDefault(u => u.Id == userId);

            //Get conflicts
            var conflicts = _git.GetConflictDetails(
                repository.Path,
                mergeRequest.BranchName
            );

            if (conflicts.Any())
            {
                return BadRequest(new
                {
                    Message = "Merge conflict detected",
                    Conflicts = conflicts
                });
            }

            //Merge
            var result = _git.MergeBranch(
                repository.Path,
                mergeRequest.BranchName,
                owner.UserName,
                owner.Email
            );

            mergeRequest.IsMerged = true;
            mergeRequest.MergedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(new { Message = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Merge failed",
                Error = ex.Message
            });
        }
    }

    [HttpPost("{repositoryId}/resolve-conflict")]
    public IActionResult ResolveConflict(Guid repositoryId, string filePath, string resolvedContent)
    {
        var userId = User.GetUserId();

        var repository = _db.therepository.FirstOrDefault(r => r.Id == repositoryId);
        if (repository == null)
            return NotFound();

        var user = _db.registrations.FirstOrDefault(u => u.Id == userId);

        var fullPath = Path.Combine(repository.Path, filePath);

        // 🔥 overwrite file with resolved content
        System.IO.File.WriteAllText(fullPath, resolvedContent);

        // 🔥 commit resolution
        _git.CommitChanges(
            repository.Path,
            $"Resolve conflict in {filePath}",
            user.UserName,
            user.Email
        );

        return Ok("Conflict resolved and committed");
    }
}