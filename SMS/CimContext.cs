using Microsoft.EntityFrameworkCore;
using SMS.Models;

namespace SMS
{
    public class CimContext : DbContext
    {
        public CimContext(DbContextOptions<CimContext> options) : base(options) { }
        public DbSet<Student> SMS_Students { get; set; }
        public DbSet<Teacher> SMS_Teachers { get; set; }
        public DbSet<User> SMS_Users { get; set; }
        public DbSet<Course> SMS_Courses { get; set; }
        public DbSet<Enrollment> SMS_Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置Enrollment的外键关系
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentSID)
                .HasPrincipalKey(s => s.SID);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseCID)
                .HasPrincipalKey(c => c.CID);

            // 配置Course的外键关系到Teacher
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .HasPrincipalKey(t => t.Id)
                .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Student || e.Entity is Teacher || e.Entity is User || e.Entity is Course || e.Entity is Enrollment);
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreateDate").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdateDate").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdateDate").CurrentValue = DateTime.UtcNow;
                }
            }
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Student || e.Entity is Teacher || e.Entity is User || e.Entity is Course || e.Entity is Enrollment);
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreateDate").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdateDate").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdateDate").CurrentValue = DateTime.UtcNow;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}