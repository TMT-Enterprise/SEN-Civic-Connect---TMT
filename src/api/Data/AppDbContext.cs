using CivicConnect.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Requester> Requesters => Set<Requester>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<RequestStatusEntity> RequestStatuses => Set<RequestStatusEntity>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.ToTable("service_request");
            entity.HasKey(r => r.RequestId);

            entity.Property(r => r.RequestId)
                .HasColumnName("request_id")
                .ValueGeneratedOnAdd();

            entity.Property(r => r.RequesterId).HasColumnName("requester_id");
            entity.Property(r => r.CategoryId).HasColumnName("category_id");
            entity.Property(r => r.AssignedEmployeeId).HasColumnName("assigned_employee_id");
            entity.Property(r => r.CurrentStatusId).HasColumnName("current_status_id");

            entity.Property(r => r.Description)
                .HasColumnName("description")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(r => r.SubmittedAt)
                .HasColumnName("submitted_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.Property(r => r.DueAt)
                .HasColumnName("due_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(r => r.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.Property(r => r.ClosedAt)
                .HasColumnName("closed_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.HasOne(r => r.CurrentStatus)
                .WithMany(s => s.ServiceRequests)
                .HasForeignKey(r => r.CurrentStatusId)
                .HasPrincipalKey(s => s.StatusId);
        });

        modelBuilder.Entity<Requester>(entity =>
        {
            entity.ToTable("requester");
            entity.HasKey(x => x.RequesterId);

            entity.Property(x => x.RequesterId)
                .HasColumnName("requester_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.ToTable("service_category");
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.CategoryId).HasColumnName("category_id");
            entity.Property(c => c.Type).HasColumnName("type");
            entity.Property(c => c.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<RequestStatusEntity>(entity =>
        {
            entity.ToTable("request_status");
            entity.HasKey(s => s.StatusId);
            entity.Property(s => s.StatusId).HasColumnName("status_id");
            entity.Property(s => s.Name).HasColumnName("name");
        });

        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.ToTable("employee_role");
            entity.HasKey(x => x.RoleId);

            entity.Property(x => x.RoleId)
                .HasColumnName("role_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employee");
            entity.HasKey(x => x.EmployeeId);

            entity.Property(x => x.EmployeeId)
                .HasColumnName("employee_id")
                .ValueGeneratedOnAdd();

            entity.Property(x => x.RoleId).HasColumnName("role_id");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.IsActive).HasColumnName("is_active");

            entity.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId);
        });
    }
}