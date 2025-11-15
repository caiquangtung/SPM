using Microsoft.EntityFrameworkCore;
using FileModel = file_service.Models.File;
using file_service.Models;

namespace file_service.Data;

public class FileDbContext : DbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
    {
    }

    public DbSet<FileModel> Files { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema
        modelBuilder.HasDefaultSchema("spm_file");

        // Configure File entity
        modelBuilder.Entity<FileModel>(entity =>
        {
            entity.ToTable("files");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.OriginalName)
                .HasColumnName("original_name")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.StoredName)
                .HasColumnName("stored_name")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Size)
                .HasColumnName("size")
                .IsRequired();

            entity.Property(e => e.StoragePath)
                .HasColumnName("storage_path")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.UploadedBy)
                .HasColumnName("uploaded_by")
                .IsRequired();

            entity.Property(e => e.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            entity.HasIndex(e => e.UploadedBy)
                .HasDatabaseName("idx_files_uploaded_by");

            entity.HasIndex(e => e.UploadedAt)
                .HasDatabaseName("idx_files_uploaded_at");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("idx_files_is_deleted");
        });

        // Configure TaskAttachment entity
        modelBuilder.Entity<TaskAttachment>(entity =>
        {
            entity.ToTable("task_attachments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.TaskId)
                .HasColumnName("task_id")
                .IsRequired();

            entity.Property(e => e.FileId)
                .HasColumnName("file_id")
                .IsRequired();

            entity.Property(e => e.UploadedBy)
                .HasColumnName("uploaded_by")
                .IsRequired();

            entity.Property(e => e.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.File)
                .WithMany(f => f.TaskAttachments)
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TaskId)
                .HasDatabaseName("idx_task_attachments_task_id");

            entity.HasIndex(e => e.FileId)
                .HasDatabaseName("idx_task_attachments_file_id");
        });
    }
}

