using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Registration.Persistence.entity
{
    public partial class EmailVerification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid E-mail address.")] // Email validation
        public string Email { get; set; }
        public DateTime Expiry { get; set; }

        [Required]
        [Column(TypeName = "varchar(6)")] // Limiting code to 6 digits
        public string Code { get; set; }

        // public bool IsVerified { get; set; } = false;  // Flag to track if the code was used successfully
        //public DateTime CreatedAt { get; set; } = DateTime.Now;  // Automatically setting creation time

    }

}