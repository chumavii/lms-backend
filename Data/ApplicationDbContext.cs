using LmsApi.Models;
using LmsApi.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace LmsApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<InstructorApprovalRequest> InstructorApprovalRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser Configuration
            builder.Entity<ApplicationUser>(b =>
            {
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.FullName).HasMaxLength(100);
            });

            // Course Configuration
            builder.Entity<Course>(b =>
            {
                b.HasKey(c => c.Id);

                b.Property(c => c.Title).IsRequired().HasMaxLength(200);
                b.Property(c => c.Status).HasDefaultValue(CourseStatus.Published);
                
                b.HasOne(c => c.Instructor)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict); // prevent deleting instructor if courses exist

                b.HasMany(c => c.Enrollments)
                .WithOne(e => e.Course)
                .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(c => c.Modules)
                .WithOne(e => e.Course)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            //Module Configuration
            builder.Entity<Module>(b =>
            {
                b.HasKey(m => m.Id);
                b.Property(m => m.Title).IsRequired().HasMaxLength(200);

                b.HasMany(c => c.Lessons)
                .WithOne(e => e.Module)
                .HasForeignKey(c => c.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            });


            // Lesson Configuration
            builder.Entity<Lesson>(b =>
            {
                b.HasKey(l => l.Id);
                b.Property(l => l.Title).IsRequired().HasMaxLength(200);
                b.Property(l => l.ContentUrl).HasMaxLength(500);
            });

            // Enrollment Configuration
            builder.Entity<Enrollment>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique(); // prevent duplicate enrollments

                b.HasOne(e => e.User)
                 .WithMany(u => u.Enrollments)
                 .HasForeignKey(e => e.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.Course)
                 .WithMany(c => c.Enrollments)
                 .HasForeignKey(e => e.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(e => e.Progress).HasDefaultValue(0);
                b.Property(e => e.CreatedAt);
            });

            // Instructor Approval Configuration
            builder.Entity<InstructorApprovalRequest>(b =>
            {
                b.HasKey(r => r.Id);

                b.HasOne(r => r.User)
                 .WithMany(u => u.ApprovalRequests)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(r => r.Status).HasDefaultValue(ApprovalStatus.Pending);
                b.Property(r => r.RequestedAt);
            });
        }
    }
}
