using System.ComponentModel.DataAnnotations.Schema;

namespace OvertimeControlCodeFirst.Models
{
    public class SalaryCategoryValue
    {
        public int SalaryCategoryValueId { get; set; }
        public int SalaryCategoryId { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseSalary { get; set; }
        public int FromMonth { get; set; }
        public int FromYear { get; set; } 
        public int? ToMonth { get; set; }   // si sigue vigente, puede ser null
        public int? ToYear { get; set; }
        public required SalaryCategory SalaryCategory { get; set; }
    }
}
