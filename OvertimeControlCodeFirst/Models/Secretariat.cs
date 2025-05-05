using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class Secretariat
    {
        public int SecretariatId { get; set; }

        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "No puede estar vacío")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Area> Areas { get; set; } = new List<Area>();

    }
}
