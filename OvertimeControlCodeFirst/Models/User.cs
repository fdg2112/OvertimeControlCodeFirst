using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class User
    {
        public int UserId { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(255)]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public Role Role { get; set; } = new Role();

        public int AreaId { get; set; }
        public Area Area { get; set; } = new Area();

        public int SecretariatId { get; set; }
        public Secretariat Secretariat { get; set; } = new Secretariat();

        public virtual ICollection<LoginAudit> LoginAudits { get; set; } = new List<LoginAudit>();
    }
}
