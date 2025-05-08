using OvertimeControlCodeFirst.Enums;

namespace ControlHorasExtras.Models
{
    public class OvertimeExpenseView
    {
        public HourType HourType { get; set; }
        public decimal TotalExpense { get; set; }
    }
}
