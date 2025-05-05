using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class Area
    {
        public int AreaId { get; set; }

        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "No puede estar vacío")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int SecretariatId { get; set; }

        public required Secretariat Secretariat { get; set; }
    }
}
