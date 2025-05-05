namespace OvertimeControlCodeFirst.Models
{
    public class WorkActivity
    {
        public int WorkActivityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int WorkplaceId { get; set; }
        public virtual Workplace Workplace { get; set; } = null!;
    }
}
