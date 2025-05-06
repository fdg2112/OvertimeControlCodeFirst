using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class Workplace
    {
        public int WorkplaceId { get; set; }
        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "No puede estar vacío")]
        public string Name { get; set; } = string.Empty;

        public ICollection<WorkActivity> WorkActivities { get; set; } = new List<WorkActivity>();
    }
}
