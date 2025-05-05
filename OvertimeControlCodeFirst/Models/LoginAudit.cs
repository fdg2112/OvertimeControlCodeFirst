namespace OvertimeControlCodeFirst.Models
{
    public class LoginAudit
    {
        public int LoginAuditId { get; set; }

        public int UserId { get; set; }

        public DateTime LoginDate { get; set; }

        public DateTime LogoutDate { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;

    }
}
