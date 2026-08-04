namespace ErrorMailTool.BLL.Models;

public sealed class ErrorMailListItemDto
{
    public required string Id { get; init; }

    public required string Category { get; init; }

    public required string SystemName { get; init; }

    public required string CustomerName { get; init; }

    public required string StoreName { get; init; }

    public required string Version { get; init; }

    public DateTime? OccurredAt { get; init; }

    public required string Subject { get; init; }

    public int AttachmentCount { get; init; }

    public bool IsContentComplete { get; init; }
}
