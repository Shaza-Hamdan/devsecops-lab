namespace GitFile.DTO
{
    public record VerifyCodeRequest
     (
         string Email,
         string Code,
         DateTime Expiry
     );
}