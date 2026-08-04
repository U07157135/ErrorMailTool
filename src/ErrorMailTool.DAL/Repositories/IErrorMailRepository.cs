using ErrorMailTool.DAL.Models;

namespace ErrorMailTool.DAL.Repositories;

public interface IErrorMailRepository
{
    IReadOnlyList<ErrorMailRecord> GetAll(DateOnly? startDate = null, DateOnly? endDate = null);

    ErrorMailRecord? GetById(string id);

    DateTime? GetLastSyncedAt();
}
