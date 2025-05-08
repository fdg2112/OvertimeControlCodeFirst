using OvertimeControlCodeFirst.Models;

namespace OvertimeControlCodeFirst.Helpers
{
    public static class SalaryHelper
    {
        public static decimal GetValidBaseSalary(ICollection<SalaryCategoryValue> values, int month, int year)
        {
            return values.FirstOrDefault(v =>
                (v.FromYear < year || (v.FromYear == year && v.FromMonth <= month)) &&
                (v.ToYear == null || v.ToYear > year || (v.ToYear == year && (v.ToMonth == null || v.ToMonth >= month)))
            )?.BaseSalary ?? 0;
        }
    }
}
