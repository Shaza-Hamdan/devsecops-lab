using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;
using GitFile.Extensions;
using Persistence.entity;
using Registration.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using LibGit2Sharp;

[ApiController]
[Route("api/[controller]")]
public class RepositoryController : ControllerBase
{
    private readonly IRepositoryService _repositoryService;
    private readonly AppDBContext _db;
    private readonly IGitService _git;
    public RepositoryController(IRepositoryService repositoryService, AppDBContext db, IGitService git)
    {
        _repositoryService = repositoryService;
        _db = db;
        _git = git;
    }

    [HttpPost("Create New Repository")]
    public IActionResult CreateRepository(string name)
    {
        try
        {
            var userId = User.GetUserId();

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Repository name is required.");

            var repoId = _repositoryService.CreateRepository(userId, name);

            return Ok(new
            {
                Message = "Repository created successfully.",
                RepositoryId = repoId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Something went wrong.",
                Error = ex.Message
            });
        }
    }

    [HttpPost("{repositoryId}/collaborators")]
    public IActionResult AddCollaborator(Guid repositoryId, Guid collaboratorId, string role)
    {
        try
        {
            var ownerId = User.GetUserId();

            // Parse the string into the enum
            if (!Enum.TryParse<RepositoryRole>(role, true, out var roleEnum))
                return BadRequest("Invalid role. Allowed roles: Owner, Collaborator, Guest");

            _repositoryService.AddCollaborator(repositoryId, ownerId, collaboratorId, roleEnum);

            return Ok("Collaborator added successfully.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Unexpected error.", Error = ex.Message });
        }
    }

    [HttpGet("{repoId}/openrepo")]
    public IActionResult GetRepository(Guid repoId)
    {
        var repo = _db.therepository
            .Include(r => r.Collaborators)
            .FirstOrDefault(r => r.Id == repoId);

        if (repo == null)
            return NotFound("Repository not found.");

        return Ok(repo); //returns info about the repo so the front end can load the repo page
    }

    [HttpPost("{repoId}/branches")]
    public IActionResult CreateBranch(Guid repoId, string branchName)
    {
        var repo = _db.therepository
            .Include(r => r.Collaborators)
            .FirstOrDefault(r => r.Id == repoId);

        if (repo == null)
            return NotFound();

        _git.CreateBranch(repo.Path, branchName);

        return Ok("Branch created");
    }
}