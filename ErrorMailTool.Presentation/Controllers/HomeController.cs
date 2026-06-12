using System.Diagnostics;
using ErrorMailTool.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using ErrorMailTool.Presentation.Models;

namespace ErrorMailTool.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly IErrorMailService _errorMailService;

    public HomeController(IErrorMailService errorMailService)
    {
        _errorMailService = errorMailService;
    }

    public IActionResult Index(DateOnly? startDate, DateOnly? endDate)
    {
        var dashboard = _errorMailService.GetDashboard(startDate, endDate);
        return View(dashboard);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sync(DateOnly? startDate, DateOnly? endDate)
    {
        try
        {
            var result = _errorMailService.SyncErrorMails();
            TempData["SyncMessage"] = result.Summary;

            if (result.Errors.Count > 0)
            {
                TempData["SyncErrors"] = string.Join(Environment.NewLine, result.Errors.Take(5));
            }
        }
        catch (Exception ex)
        {
            TempData["SyncError"] = $"同步失敗：{ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { startDate, endDate });
    }

    public IActionResult Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var detail = _errorMailService.GetDetail(id);
        return detail is null ? NotFound() : View(detail);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
