using System.Security.Claims;

public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _allowedRoles;

    public RoleAuthorizationMiddleware(RequestDelegate next, string[] allowedRoles)
    {
        _next = next;
        _allowedRoles = allowedRoles;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User is not authenticated.");
            return;
        }

        var userRoles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        if (!_allowedRoles.Any(role => userRoles.Contains(role)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("You do not have permission to access this resource.");
            return;
        }

        await _next(context);
    }
}