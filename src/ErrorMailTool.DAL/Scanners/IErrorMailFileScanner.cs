using ErrorMailTool.DAL.Models;

namespace ErrorMailTool.DAL.Scanners;

public interface IErrorMailFileScanner
{
    IReadOnlyList<ErrorMailRecord> ScanAll();
}
