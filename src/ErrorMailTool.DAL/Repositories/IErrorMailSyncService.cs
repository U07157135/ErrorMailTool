using ErrorMailTool.DAL.Models;

namespace ErrorMailTool.DAL.Repositories;

public interface IErrorMailSyncService
{
    ErrorMailSyncResult SyncFromFileSystem();
}
