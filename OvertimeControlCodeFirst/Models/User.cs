using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class User
    {
        public int UserId { get; set; }

        [StringLength(100)]
        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(255)]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "El nombre del usuario es obligatorio.")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "El apellido del usuario es obligatorio.")]
        public string LastName { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public required Role Role { get; set; }

        public int? AreaId { get; set; }
        public virtual Area? Area { get; set; }

        public int? SecretariatId { get; set; }
        public virtual Secretariat? Secretariat { get; set; }

        public virtual ICollection<LoginAudit> LoginAudits { get; set; } = new List<LoginAudit>();
    }
}
