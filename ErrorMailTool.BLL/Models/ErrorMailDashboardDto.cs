namespace ErrorMailTool.BLL.Models;

public sealed class ErrorMailDashboardDto
{
    public required string BackupPath { get; init; }

    public bool BackupPathExists { get; init; }

    public bool DatabaseAvailable { get; init; } = true;

    public string? DatabaseErrorMessage { get; init; }

    public DateTime? LastSyncedAt { get; init; }

    public int TotalCount { get; init; }

    public int IncompleteCount { get; init; }

    public DateOnly? SelectedStartDate { get; init; }

    public DateOnly? SelectedEndDate { get; init; }

    public bool IsDateFiltered => SelectedStartDate.HasValue || SelectedEndDate.HasValue;

    public IReadOnlyList<ErrorMailListItemDto> Items { get; init; } = [];

    public IReadOnlyList<ErrorMailTrendPointDto> TrendPoints { get; init; } = [];
}
