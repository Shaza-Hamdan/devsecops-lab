using System.ComponentModel.DataAnnotations;

public class UpdateRoleRequest
{
    [Required]
    public string NewRole { get; set; }
}