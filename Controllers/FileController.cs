using GitFile.DTO;
using GitFile.Extensions;
using Microsoft.AspNetCore.Mvc;
using GitFile.FileCreate;
using Microsoft.AspNetCore.Authorization;
using Registration.Persistence.Repository;

[ApiController]
[Route("api/files")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileService _files;
    private readonly AppDBContext _db;
    private readonly IGitService _git;
    public FileController(IFileService files, AppDBContext db, IGitService git)
    {
        _files = files;
        _db = db;
        _git = git;
    }

    [HttpPost("{repositoryId}/createfile")]
    public IActionResult Create(Guid repositoryId, [FromBody] CreateFileDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.FileName))
                return BadRequest("File name is required.");
            Guid userId = User.GetUserId();
            var fileId = _files.CreateFile(repositoryId, userId, dto.FileName, dto.Content);

            return Ok(new
            {
                FileId = fileId,
                Message = "File created successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Failed to create file.",
                Error = ex.Message
            });
        }
    }

    [HttpPost("edit")]
    public IActionResult EditFile(Guid repoId, Guid fileId, string branchName, string content, Guid userId)
    {
        var repo = _db.therepository.FirstOrDefault(r => r.Id == repoId);
        var file = _db.repositoryFile.FirstOrDefault(f => f.Id == fileId);
        var user = _db.registrations.FirstOrDefault(u => u.Id == userId);

        if (repo == null || file == null || user == null)
            return NotFound();

        _files.EditFile(repo.Path, branchName, file.Path, content, user.UserName, user.Email);

        return Ok("File updated and committed");
    }
}
