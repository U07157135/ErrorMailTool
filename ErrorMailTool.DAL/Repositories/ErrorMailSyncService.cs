using ErrorMailTool.DAL.Data;
using ErrorMailTool.DAL.Entities;
using ErrorMailTool.DAL.Models;
using ErrorMailTool.DAL.Scanners;
using Microsoft.EntityFrameworkCore;

namespace ErrorMailTool.DAL.Repositories;

public sealed class ErrorMailSyncService : IErrorMailSyncService
{
    private readonly ErrorMailDbContext _dbContext;
    private readonly IErrorMailFileScanner _fileScanner;

    public ErrorMailSyncService(ErrorMailDbContext dbContext, IErrorMailFileScanner fileScanner)
    {
        _dbContext = dbContext;
        _fileScanner = fileScanner;
    }

    public ErrorMailSyncResult SyncFromFileSystem()
    {
        var result = new ErrorMailSyncResult();
        var errors = new List<string>();
        IReadOnlyList<ErrorMailRecord> scannedRecords;

        try
        {
            scannedRecords = _fileScanner.ScanAll();
        }
        catch (Exception ex)
        {
            result.FailedCount = 1;
            result.Errors = [$"掃描 ErrorMail 資料夾失敗：{ex.Message}"];
            return result;
        }

        foreach (var record in scannedRecords)
        {
            try
            {
                var entity = _dbContext.ErrorMails
                    .Include(mail => mail.Attachments)
                    .FirstOrDefault(mail => mail.Id == record.Id);

                if (entity is null)
                {
                    _dbContext.ErrorMails.Add(CreateEntity(record));
                    result.AddedCount++;
                    continue;
                }

                if (string.Equals(entity.ContentHash, record.ContentHash, StringComparison.Ordinal))
                {
                    result.SkippedCount++;
                    continue;
                }

                UpdateEntity(entity, record);
                result.UpdatedCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                errors.Add($"{record.FolderName}: {ex.Message}");
            }
        }

        _dbContext.SaveChanges();
        result.Errors = errors;
        return result;
    }

    private static ErrorMailEntity CreateEntity(ErrorMailRecord record)
    {
        var now = DateTime.Now;
        var entity = new ErrorMailEntity
        {
            Id = record.Id,
            FolderPath = record.FolderPath,
            FolderName = record.FolderName,
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
            ContentHash = record.ContentHash,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var attachment in record.Attachments)
        {
            entity.Attachments.Add(CreateAttachment(record.Id, attachment));
        }

        return entity;
    }

    private static void UpdateEntity(ErrorMailEntity entity, ErrorMailRecord record)
    {
        entity.FolderPath = record.FolderPath;
        entity.FolderName = record.FolderName;
        entity.Category = record.Category;
        entity.SystemName = record.SystemName;
        entity.CustomerName = record.CustomerName;
        entity.StoreName = record.StoreName;
        entity.Version = record.Version;
        entity.OccurredAt = record.OccurredAt;
        entity.Subject = record.Subject;
        entity.From = record.From;
        entity.PostedDate = record.PostedDate;
        entity.Body = record.Body;
        entity.HasContentFile = record.HasContentFile;
        entity.IsContentComplete = record.IsContentComplete;
        entity.ContentHash = record.ContentHash;
        entity.UpdatedAt = DateTime.Now;

        entity.Attachments.Clear();
        foreach (var attachment in record.Attachments)
        {
            entity.Attachments.Add(CreateAttachment(record.Id, attachment));
        }
    }

    private static ErrorMailAttachmentEntity CreateAttachment(string errorMailId, ErrorMailAttachment attachment)
    {
        return new ErrorMailAttachmentEntity
        {
            ErrorMailId = errorMailId,
            FileName = attachment.FileName,
            FullPath = attachment.FullPath,
            Length = attachment.Length
        };
    }
}
