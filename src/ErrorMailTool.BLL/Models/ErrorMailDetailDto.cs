namespace ErrorMailTool.BLL.Models;

public sealed class ErrorMailDetailDto
{
    public required string Id { get; init; }

    public required string FolderName { get; init; }

    public required string FolderPath { get; init; }

    public required string Category { get; init; }

    public required string SystemName { get; init; }

    public required string CustomerName { get; init; }

    public required string StoreName { get; init; }

    public required string Version { get; init; }

    public DateTime? OccurredAt { get; init; }

    public required string Subject { get; init; }

    public required string From { get; init; }

    public DateTime? PostedDate { get; init; }

    public required string Body { get; init; }

    public bool HasContentFile { get; init; }

    public bool IsContentComplete { get; init; }

    public IReadOnlyList<ErrorMailAttachmentDto> Attachments { get; init; } = [];
}
