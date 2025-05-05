using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OvertimeControlCodeFirst.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        [Range(1, 999)]
        public int RecordNumber { get; set; } // Legajo
        [Required, MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "No puede estar vacío")]
        public string Name { get; set; } = string.Empty;
        [Required, MinLength(1), RegularExpression(@"\S+.*", ErrorMessage = "No puede estar vacío")]
        public string LastName { get; set; } = string.Empty;
        public int? AreaId { get; set; }
        public int SecretariatId { get; set; }
        public int SalaryCategoryId { get; set; }
        public DateOnly AdmissionDate { get; set; }

        public virtual Area? Area { get; set; }
        public required Secretariat Secretariat { get; set; }
        public required SalaryCategory SalaryCategory { get; set; }
        public virtual ICollection<Overtime> Overtimes { get; set; } = new List<Overtime>();
    }
}
