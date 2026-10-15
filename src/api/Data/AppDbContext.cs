using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Requester> Requesters => Set<Requester>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.ToTable("employee_role");
            entity.HasKey(x => x.RoleId);
            entity.Property(x => x.RoleId).HasColumnName("role_id").ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Requester>(entity =>
        {
            entity.ToTable("requester");
            entity.HasKey(x => x.RequesterId);
            entity.Property(x => x.RequesterId).HasColumnName("requester_id").ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employee");
            entity.HasKey(x => x.EmployeeId);
            entity.Property(x => x.EmployeeId).HasColumnName("employee_id").ValueGeneratedOnAdd();
            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });
    }
}