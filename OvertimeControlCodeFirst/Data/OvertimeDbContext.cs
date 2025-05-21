using OvertimeControlCodeFirst.Models;
using Microsoft.EntityFrameworkCore;
using ControlHorasExtras.Models;

namespace OvertimeControlCodeFirst.Data;

public class OvertimeDbContext : DbContext
{
    public OvertimeDbContext(DbContextOptions<OvertimeDbContext> options) : base(options) { }

    public DbSet<Area> Areas { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<HourLimit> HoursLimit { get; set; }
    public DbSet<LoginAudit> LoginAudits { get; set; }
    public DbSet<Overtime> Overtimes { get; set; }
    public DbSet<Role> Roles { get; set; }    
    public DbSet<SalaryCategory> SalaryCategories { get; set; }
    public DbSet<SalaryCategoryValue> SalaryCategoryValues { get; set; }
    public DbSet<Secretariat> Secretariats { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WorkActivity> WorkActivities { get; set; }
    public DbSet<Workplace> Workplaces { get; set; }
    public DbSet<OvertimeExpenseView> OvertimeExpenseView { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de relaciones y restricciones de borrado
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Secretariat)
            .WithMany()
            .HasForeignKey(e => e.SecretariatId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Overtime>()
            .HasOne(o => o.Employee)
            .WithMany()
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Overtime>()
            .HasOne(o => o.Area)
            .WithMany()
            .HasForeignKey(o => o.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Overtime>()
            .HasOne(o => o.Secretariat)
            .WithMany()
            .HasForeignKey(o => o.SecretariatId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Overtime>()
            .HasOne(o => o.Employee)
            .WithMany(e => e.Overtimes)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Overtime>()
            .HasOne(o => o.WorkActivity)
            .WithMany()
            .HasForeignKey(o => o.WorkActivityId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<User>()
            .HasOne(u => u.Secretariat)
            .WithMany()
            .HasForeignKey(u => u.SecretariatId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalaryCategory>()
            .HasMany(sc => sc.SalaryCategoryValues)
            .WithOne(scv => scv.SalaryCategory)
            .HasForeignKey(scv => scv.SalaryCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OvertimeExpenseView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("OvertimeExpenseView");
            entity.Property(e => e.HourType).HasConversion<int>();
            entity.Property(e => e.TotalExpense).HasColumnType("decimal(18,2)");
        });
    }
}

