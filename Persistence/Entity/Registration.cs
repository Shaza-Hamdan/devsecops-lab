using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Registration.Persistence.entity;

namespace Registration.Persistence.entity
{
    public class RegistrationUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string UserName { get; set; }


        [EmailAddress(ErrorMessage = "Please enter a valid E-mail address.")] //validate the input as an Email Address
        public string Email { get; set; }


        public string PasswordHash { get; set; }
        public string Salt { get; set; }

        public string PhoneNumber { get; set; }

        [ForeignKey("role")]
        public Guid roleId { get; set; }
        public RoleEntity role { get; set; }
    }
}
