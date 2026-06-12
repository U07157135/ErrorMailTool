using ErrorMailTool.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ErrorMailTool.DAL.Data;

public sealed class ErrorMailDbContext : DbContext
{
    public ErrorMailDbContext(DbContextOptions<ErrorMailDbContext> options)
        : base(options)
    {
    }

    public DbSet<ErrorMailEntity> ErrorMails => Set<ErrorMailEntity>();

    public DbSet<ErrorMailAttachmentEntity> ErrorMailAttachments => Set<ErrorMailAttachmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErrorMailEntity>(entity =>
        {
            entity.ToTable("ErrorMails");
            entity.HasKey(mail => mail.Id);
            entity.Property(mail => mail.Id).HasMaxLength(32).ValueGeneratedNever();
            entity.Property(mail => mail.FolderPath).HasMaxLength(1024).IsRequired();
            entity.Property(mail => mail.FolderName).HasMaxLength(512).IsRequired();
            entity.Property(mail => mail.Category).HasMaxLength(100).IsRequired();
            entity.Property(mail => mail.SystemName).HasMaxLength(200).IsRequired();
            entity.Property(mail => mail.CustomerName).HasMaxLength(300).IsRequired();
            entity.Property(mail => mail.StoreName).HasMaxLength(300).IsRequired();
            entity.Property(mail => mail.Version).HasMaxLength(100).IsRequired();
            entity.Property(mail => mail.Subject).HasMaxLength(1000).IsRequired();
            entity.Property(mail => mail.From).HasMaxLength(500).IsRequired();
            entity.Property(mail => mail.Body).IsRequired();
            entity.Property(mail => mail.ContentHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(mail => mail.FolderPath).IsUnique();
            entity.HasIndex(mail => mail.OccurredAt);
            entity.HasMany(mail => mail.Attachments)
                .WithOne(attachment => attachment.ErrorMail)
                .HasForeignKey(attachment => attachment.ErrorMailId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ErrorMailAttachmentEntity>(entity =>
        {
            entity.ToTable("ErrorMailAttachments");
            entity.HasKey(attachment => attachment.Id);
            entity.Property(attachment => attachment.ErrorMailId).HasMaxLength(32).IsRequired();
            entity.Property(attachment => attachment.FileName).HasMaxLength(512).IsRequired();
            entity.Property(attachment => attachment.FullPath).HasMaxLength(1024).IsRequired();
            entity.HasIndex(attachment => attachment.ErrorMailId);
        });
    }
}
