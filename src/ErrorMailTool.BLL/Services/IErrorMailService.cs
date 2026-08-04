using ErrorMailTool.BLL.Models;

namespace ErrorMailTool.BLL.Services;

public interface IErrorMailService
{
    ErrorMailDashboardDto GetDashboard(DateOnly? startDate = null, DateOnly? endDate = null, int days = 14);

    ErrorMailDetailDto? GetDetail(string id);

    ErrorMailSyncResultDto SyncErrorMails();
}
