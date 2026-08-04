namespace ErrorMailTool.DAL.Models;

public sealed class ErrorMailSyncResult
{
    public int AddedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int SkippedCount { get; set; }

    public int FailedCount { get; set; }

    public IReadOnlyList<string> Errors { get; set; } = [];
}
