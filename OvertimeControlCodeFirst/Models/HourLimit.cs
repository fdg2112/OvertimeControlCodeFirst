namespace OvertimeControlCodeFirst.Models
{
    public class HourLimit
    {
        public int HourLimitId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Limit { get; set; }
        public int AreaId { get; set; }
        public required Area Area { get; set; }
    }
}
