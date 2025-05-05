using System.ComponentModel.DataAnnotations;

namespace OvertimeControlCodeFirst.Models
{
    public class SalaryCategory
    {
        public int SalaryCategoryId { get; set; }

        [Range(1, 10)]
        public int Number { get; set; }
        public virtual ICollection<SalaryCategoryValue> Values { get; set; } = new List<SalaryCategoryValue>();

    }
}
