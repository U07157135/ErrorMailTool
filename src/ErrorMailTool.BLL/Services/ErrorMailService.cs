using ErrorMailTool.BLL.Models;
using ErrorMailTool.DAL.Models;
using ErrorMailTool.DAL.Repositories;

namespace ErrorMailTool.BLL.Services;

public sealed class ErrorMailService : IErrorMailService
{
    private readonly IErrorMailRepository _repository;
    private readonly IErrorMailSyncService _syncService;
    private readonly string _backupPath;

    public ErrorMailService(IErrorMailRepository repository, IErrorMailSyncService syncService, string backupPath)
    {
        _repository = repository;
        _syncService = syncService;
        _backupPath = backupPath;
    }

    public ErrorMailDashboardDto GetDashboard(DateOnly? startDate = null, DateOnly? endDate = null, int days = 14)
    {
        var normalizedRange = NormalizeDateRange(startDate, endDate);
        IReadOnlyList<ErrorMailRecord> records;
        DateTime? lastSyncedAt = null;

        try
        {
            records = _repository.GetAll(normalizedRange.StartDate, normalizedRange.EndDate);
            lastSyncedAt = _repository.GetLastSyncedAt();
        }
        catch (Exception ex)
        {
            return new ErrorMailDashboardDto
            {
                BackupPath = _backupPath,
                BackupPathExists = Directory.Exists(_backupPath),
                DatabaseAvailable = false,
                DatabaseErrorMessage = ex.Message,
                SelectedStartDate = normalizedRange.StartDate,
                SelectedEndDate = normalizedRange.EndDate
            };
        }

        var trendPoints = BuildTrend(records, normalizedRange.StartDate, normalizedRange.EndDate, days);

        return new ErrorMailDashboardDto
        {
            BackupPath = _backupPath,
            BackupPathExists = Directory.Exists(_backupPath),
            DatabaseAvailable = true,
            LastSyncedAt = lastSyncedAt,
            TotalCount = records.Count,
            IncompleteCount = records.Count(mail => !mail.IsContentComplete),
            SelectedStartDate = normalizedRange.StartDate,
            SelectedEndDate = normalizedRange.EndDate,
            Items = records.Select(MapListItem).ToList(),
            TrendPoints = trendPoints
        };
    }

    public ErrorMailDetailDto? GetDetail(string id)
    {
        var record = _repository.GetById(id);
        return record is null ? null : MapDetail(record);
    }

    public ErrorMailSyncResultDto SyncErrorMails()
    {
        var result = _syncService.SyncFromFileSystem();
        return new ErrorMailSyncResultDto
        {
            AddedCount = result.AddedCount,
            UpdatedCount = result.UpdatedCount,
            SkippedCount = result.SkippedCount,
            FailedCount = result.FailedCount,
            Errors = result.Errors
        };
    }

    private static (DateOnly? StartDate, DateOnly? EndDate) NormalizeDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return (endDate, startDate);
        }

        return (startDate, endDate);
    }

    private static IReadOnlyList<ErrorMailTrendPointDto> BuildTrend(
        IReadOnlyList<ErrorMailRecord> records,
        DateOnly? startDate,
        DateOnly? endDate,
        int days)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = startDate ?? (endDate?.AddDays(-(Math.Clamp(days, 1, 90) - 1)) ?? today.AddDays(-(Math.Clamp(days, 1, 90) - 1)));
        var end = endDate ?? (startDate.HasValue ? today : today);

        if (start > end)
        {
            (start, end) = (end, start);
        }

        var dateCount = end.DayNumber - start.DayNumber + 1;
        var counts = records
            .Where(mail => mail.OccurredAt.HasValue)
            .GroupBy(mail => DateOnly.FromDateTime(mail.OccurredAt!.Value.Date))
            .ToDictionary(group => group.Key, group => group.Count());

        return Enumerable.Range(0, dateCount)
            .Select(offset =>
            {
                var date = start.AddDays(offset);
                return new ErrorMailTrendPointDto
                {
                    Date = date,
                    Count = counts.TryGetValue(date, out var count) ? count : 0
                };
            })
            .ToList();
    }

    private static ErrorMailListItemDto MapListItem(ErrorMailRecord record)
    {
        return new ErrorMailListItemDto
        {
            Id = record.Id,
            Category = record.Category,
            SystemName = record.SystemName,
            CustomerName = record.CustomerName,
            StoreName = record.StoreName,
            Version = record.Version,
            OccurredAt = record.OccurredAt,
            Subject = record.Subject,
            AttachmentCount = record.Attachments.Count,
            IsContentComplete = record.IsContentComplete
        };
    }

    private static ErrorMailDetailDto MapDetail(ErrorMailRecord record)
    {
        return new ErrorMailDetailDto
        {
            Id = record.Id,
            FolderName = record.FolderName,
            FolderPath = record.FolderPath,
            Category = record.Category,
            SystemName = record.SystemName,
            CustomerName = record.CustomerName,
            StoreName = record.StoreName,
            Version = record.Version,
            OccurredAt = record.OccurredAt,
            Subject = record.Subject,
            From = record.From,
            PostedDate = record.PostedDate,
            Body = record.Body,
            HasContentFile = record.HasContentFile,
            IsContentComplete = record.IsContentComplete,
            Attachments = record.Attachments.Select(attachment => new ErrorMailAttachmentDto
            {
                FileName = attachment.FileName,
                FullPath = attachment.FullPath,
                Length = attachment.Length
            }).ToList()
        };
    }
}
