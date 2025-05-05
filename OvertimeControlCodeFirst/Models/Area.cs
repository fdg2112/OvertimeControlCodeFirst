using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class Area
    {
        public int AreaId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int SecretariatId { get; set; }

        public Secretariat Secretariat { get; set; } = null!;
    }
}
