using ErrorMailTool.DAL.Data;
using ErrorMailTool.DAL.Entities;
using ErrorMailTool.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ErrorMailTool.DAL.Repositories;

public sealed class EntityFrameworkErrorMailRepository : IErrorMailRepository
{
    private readonly ErrorMailDbContext _dbContext;

    public EntityFrameworkErrorMailRepository(ErrorMailDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<ErrorMailRecord> GetAll(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _dbContext.ErrorMails
            .AsNoTracking()
            .Include(mail => mail.Attachments)
            .AsQueryable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(mail => mail.OccurredAt >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(mail => mail.OccurredAt <= end);
        }

        return query
            .OrderByDescending(mail => mail.OccurredAt ?? mail.PostedDate ?? DateTime.MinValue)
            .Select(MapRecord)
            .ToList();
    }

    public ErrorMailRecord? GetById(string id)
    {
        var entity = _dbContext.ErrorMails
            .AsNoTracking()
            .Include(mail => mail.Attachments)
            .FirstOrDefault(mail => mail.Id == id);

        return entity is null ? null : MapRecord(entity);
    }

    public DateTime? GetLastSyncedAt()
    {
        return _dbContext.ErrorMails.Any()
            ? _dbContext.ErrorMails.Max(mail => (DateTime?)mail.UpdatedAt)
            : null;
    }

    private static ErrorMailRecord MapRecord(ErrorMailEntity entity)
    {
        return new ErrorMailRecord
        {
            Id = entity.Id,
            FolderName = entity.FolderName,
            FolderPath = entity.FolderPath,
            Category = entity.Category,
            SystemName = entity.SystemName,
            CustomerName = entity.CustomerName,
            StoreName = entity.StoreName,
            Version = entity.Version,
            OccurredAt = entity.OccurredAt,
            Subject = entity.Subject,
            From = entity.From,
            PostedDate = entity.PostedDate,
            Body = entity.Body,
            ContentHash = entity.ContentHash,
            HasContentFile = entity.HasContentFile,
            IsContentComplete = entity.IsContentComplete,
            Attachments = entity.Attachments
                .OrderBy(attachment => attachment.FileName)
                .Select(attachment => new ErrorMailAttachment
                {
                    FileName = attachment.FileName,
                    FullPath = attachment.FullPath,
                    Length = attachment.Length
                })
                .ToList()
        };
    }
}
