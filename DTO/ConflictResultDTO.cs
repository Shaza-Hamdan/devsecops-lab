namespace GitFile.DTO
{
    public record ConflictResult
    (
        string FilePath,
        string MainContent,
        string BranchContent
    );
}