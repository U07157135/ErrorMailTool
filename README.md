# ErrorMailTool

ErrorMailTool is a .NET 10 ASP.NET Core MVC application for collecting, storing, and reviewing ErrorMail backups from a local folder.

## Features

- Reads ErrorMail backup folders from `D:\ErrorMailBackup`.
- Decodes `content.txt` with Big5 / CP950 support.
- Stores parsed ErrorMail data in SQL Server through Entity Framework Core.
- Shows a dashboard with date range filters, counts, trend chart, and a mail list.
- Shows detail pages with mail body and attachment metadata.
- Supports manual sync from the backup folder into the database.

## Architecture

The solution uses a three-layer structure:

- `ErrorMailTool.Presentation`: ASP.NET Core MVC UI.
- `ErrorMailTool.BLL`: dashboard, filtering, detail, and sync business logic.
- `ErrorMailTool.DAL`: file scanner, EF Core DbContext, repositories, entities, and migrations.

## Requirements

- .NET SDK 10
- SQL Server available at `localhost`
- Windows trusted connection enabled for SQL Server
- ErrorMail backup folder at `D:\ErrorMailBackup`

## Configuration

Database connection string is configured in `ErrorMailTool.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ErrorMailDb": "Server=localhost;Database=ErrorMailTool;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "ErrorMail": {
    "BackupPath": "D:\\ErrorMailBackup"
  }
}
```

Change `ErrorMail:BackupPath` if the backup folder is moved.

## Database Setup

Apply EF Core migrations:

```powershell
dotnet ef database update --project ErrorMailTool.DAL\ErrorMailTool.DAL.csproj --startup-project ErrorMailTool.Presentation\ErrorMailTool.Presentation.csproj --context ErrorMailDbContext
```

The migration creates:

- `ErrorMails`
- `ErrorMailAttachments`

## Run

```powershell
dotnet run --project ErrorMailTool.Presentation\ErrorMailTool.Presentation.csproj --urls http://localhost:5275
```

Open:

```text
http://localhost:5275
```

## Sync ErrorMail Data

After the site is running:

1. Open the dashboard.
2. Click `同步 ErrorMail`.
3. The app scans `D:\ErrorMailBackup`, parses each mail folder, and writes new or changed data to SQL Server.

Repeated syncs are idempotent. Existing records are skipped when their content hash has not changed.

## Build

```powershell
dotnet build ErrorMailTool.slnx
```
