using Domain.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

public class ApplicationDbContext
    (DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Notification> Notifications => Set<Notification>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Studentens kursrelation
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(user => user.Course)
            .WithMany(course => course.Students)
            .HasForeignKey(user => user.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lärarens kursrelation
        modelBuilder.Entity<Course>()
            .HasMany(course => course.Teachers)
            .WithMany(user => user.TeachingCourses);

        // Feedback kopplad till inlämning
        modelBuilder.Entity<Feedback>()
            .HasOne(feedback => feedback.Submission)
            .WithMany(submission => submission.Feedbacks)
            .HasForeignKey(feedback => feedback.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lärare kopplad till feedback
        modelBuilder.Entity<Feedback>()
            .HasOne(feedback => feedback.Teacher)
            .WithMany()
            .HasForeignKey(feedback => feedback.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);



        // Notis kopplad till studenten.
        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Student)
            .WithMany()
            .HasForeignKey(notification => notification.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        // Generell mottagare för notiser till elever och lärare.
        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Recipient)
            .WithMany()
            .HasForeignKey(notification => notification.RecipientId)
            .OnDelete(DeleteBehavior.NoAction);


        // Notis kopplad till feedbacken som utlöste den.
        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Feedback)
            .WithMany()
            .HasForeignKey(notification => notification.FeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notis kopplad till kursmaterialet som utlöste den.
        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Document)
            .WithMany()
            .HasForeignKey(notification => notification.DocumentId)
            .OnDelete(DeleteBehavior.NoAction);


    }

}