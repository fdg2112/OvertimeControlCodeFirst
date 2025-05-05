using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required, MaxLength(100), MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "No puede estar vacío")]
        public string Name { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
