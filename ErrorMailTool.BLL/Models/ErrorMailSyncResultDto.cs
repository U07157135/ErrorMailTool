namespace ErrorMailTool.BLL.Models;

public sealed class ErrorMailSyncResultDto
{
    public int AddedCount { get; init; }

    public int UpdatedCount { get; init; }

    public int SkippedCount { get; init; }

    public int FailedCount { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public string Summary => $"新增 {AddedCount} 筆，更新 {UpdatedCount} 筆，略過 {SkippedCount} 筆，失敗 {FailedCount} 筆";
}
