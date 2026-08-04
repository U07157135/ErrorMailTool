namespace ErrorMailTool.DAL.Entities;

public sealed class ErrorMailEntity
{
    public required string Id { get; set; }

    public required string FolderPath { get; set; }

    public required string FolderName { get; set; }

    public required string Category { get; set; }

    public required string SystemName { get; set; }

    public required string CustomerName { get; set; }

    public required string StoreName { get; set; }

    public required string Version { get; set; }

    public DateTime? OccurredAt { get; set; }

    public required string Subject { get; set; }

    public required string From { get; set; }

    public DateTime? PostedDate { get; set; }

    public required string Body { get; set; }

    public bool HasContentFile { get; set; }

    public bool IsContentComplete { get; set; }

    public required string ContentHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ErrorMailAttachmentEntity> Attachments { get; set; } = [];
}
