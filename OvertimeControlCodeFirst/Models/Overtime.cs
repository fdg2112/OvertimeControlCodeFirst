using OvertimeControlCodeFirst.Enums;

namespace OvertimeControlCodeFirst.Models
{
    public class Overtime
    {
        public int OvertimeId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int HoursQuantity { get; set; }
        public HourType HourType { get; set; }
        public int AreaId { get; set; }
        public int SecretariatId { get; set; }
        public int WorkActivityId { get; set; }

        public required Employee Employee { get; set; }
        public required Area Area { get; set; }
        public required Secretariat Secretariat { get; set; }
        public required WorkActivity WorkActivity { get; set; }

    }
}
