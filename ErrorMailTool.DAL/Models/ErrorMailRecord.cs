namespace ErrorMailTool.DAL.Models;

public sealed class ErrorMailRecord
{
    public required string Id { get; init; }

    public required string FolderName { get; init; }

    public required string FolderPath { get; init; }

    public string Category { get; init; } = "未分類";

    public string SystemName { get; init; } = "未知系統";

    public string CustomerName { get; init; } = "未知客戶";

    public string StoreName { get; init; } = "未知店別";

    public string Version { get; init; } = "未知版本";

    public DateTime? OccurredAt { get; init; }

    public string Subject { get; init; } = string.Empty;

    public string From { get; init; } = string.Empty;

    public DateTime? PostedDate { get; init; }

    public string Body { get; init; } = string.Empty;

    public string ContentHash { get; init; } = string.Empty;

    public bool HasContentFile { get; init; }

    public bool IsContentComplete { get; init; }

    public IReadOnlyList<ErrorMailAttachment> Attachments { get; init; } = [];
}
