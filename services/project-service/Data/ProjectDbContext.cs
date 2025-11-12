using Microsoft.EntityFrameworkCore;
using project_service.Models;

namespace project_service.Data;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<ProjectComment> Comments => Set<ProjectComment>();
    public DbSet<TaskEmbedding> TaskEmbeddings => Set<TaskEmbedding>();
    public DbSet<CommentEmbedding> CommentEmbeddings => Set<CommentEmbedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("spm_project");

        ConfigureProjects(modelBuilder);
        ConfigureProjectMembers(modelBuilder);
        ConfigureTasks(modelBuilder);
        ConfigureComments(modelBuilder);
        ConfigureTaskEmbeddings(modelBuilder);
        ConfigureCommentEmbeddings(modelBuilder);
    }

    private static void ConfigureProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");

            entity.Property(p => p.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasColumnType("text");

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(p => p.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(p => p.CreatedBy)
                .HasDatabaseName("idx_projects_created_by");

            entity.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("idx_projects_created_at");

            entity.HasIndex(p => p.IsActive)
                .HasDatabaseName("idx_projects_is_active");
        });
    }

    private static void ConfigureProjectMembers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("project_members");

            entity.HasKey(pm => new { pm.ProjectId, pm.UserId });

            entity.Property(pm => pm.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(pm => pm.JoinedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pm => pm.UserId)
                .HasDatabaseName("idx_project_members_user_id");

            entity.HasIndex(pm => pm.Role)
                .HasDatabaseName("idx_project_members_role");
        });
    }

    private static void ConfigureTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.ToTable("tasks");

            entity.Property(t => t.Title)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasColumnType("text");

            entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(t => t.Priority)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(t => t.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Embedding)
                .WithOne(e => e.Task)
                .HasForeignKey<TaskEmbedding>(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => t.ProjectId)
                .HasDatabaseName("idx_tasks_project_id");

            entity.HasIndex(t => t.AssignedTo)
                .HasDatabaseName("idx_tasks_assigned_to");

            entity.HasIndex(t => t.CreatedBy)
                .HasDatabaseName("idx_tasks_created_by");

            entity.HasIndex(t => t.Status)
                .HasDatabaseName("idx_tasks_status");

            entity.HasIndex(t => t.Priority)
                .HasDatabaseName("idx_tasks_priority");

            entity.HasIndex(t => t.DueDate)
                .HasDatabaseName("idx_tasks_due_date");
        });
    }

    private static void ConfigureComments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectComment>(entity =>
        {
            entity.ToTable("comments");

            entity.Property(c => c.Content)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Embedding)
                .WithOne(e => e.Comment)
                .HasForeignKey<CommentEmbedding>(e => e.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.TaskId)
                .HasDatabaseName("idx_comments_task_id");

            entity.HasIndex(c => c.UserId)
                .HasDatabaseName("idx_comments_user_id");

            entity.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("idx_comments_created_at");
        });
    }

    private static void ConfigureTaskEmbeddings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEmbedding>(entity =>
        {
            entity.ToTable("task_embeddings");

            entity.HasKey(e => e.TaskId);

            entity.Property(e => e.Embedding)
                .HasColumnType("vector(768)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureCommentEmbeddings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommentEmbedding>(entity =>
        {
            entity.ToTable("comment_embeddings");

            entity.HasKey(e => e.CommentId);

            entity.Property(e => e.Embedding)
                .HasColumnType("vector(768)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

