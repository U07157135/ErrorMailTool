using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ErrorMailTool.DAL.Models;

namespace ErrorMailTool.DAL.Scanners;

public sealed partial class FileSystemErrorMailScanner : IErrorMailFileScanner
{
    private const string ContentFileName = "content.txt";
    private static readonly Encoding Big5Encoding = CreateBig5Encoding();
    private readonly string _backupPath;

    public FileSystemErrorMailScanner(string backupPath)
    {
        _backupPath = backupPath;
    }

    public IReadOnlyList<ErrorMailRecord> ScanAll()
    {
        if (!Directory.Exists(_backupPath))
        {
            throw new DirectoryNotFoundException($"找不到 ErrorMail 備份資料夾：{_backupPath}");
        }

        return Directory.EnumerateDirectories(_backupPath)
            .Select(ParseFolder)
            .OrderByDescending(mail => mail.OccurredAt ?? mail.PostedDate ?? DateTime.MinValue)
            .ToList();
    }

    private static ErrorMailRecord ParseFolder(string folderPath)
    {
        var folderName = Path.GetFileName(folderPath);
        var nameParts = FolderBracketRegex().Matches(folderName)
            .Select(match => match.Groups[1].Value.Trim())
            .ToList();

        var content = ReadContent(folderPath);
        var timestamp = ParseFolderTimestamp(folderName);
        var attachments = Directory.EnumerateFiles(folderPath)
            .Where(path => !string.Equals(Path.GetFileName(path), ContentFileName, StringComparison.OrdinalIgnoreCase))
            .Select(path =>
            {
                var file = new FileInfo(path);
                return new ErrorMailAttachment
                {
                    FileName = file.Name,
                    FullPath = file.FullName,
                    Length = file.Length
                };
            })
            .OrderBy(file => file.FileName)
            .ToList();

        var subject = string.IsNullOrWhiteSpace(content.Subject) ? folderName : content.Subject;
        var body = content.Body;

        return new ErrorMailRecord
        {
            Id = CreateStableId(folderPath),
            FolderName = folderName,
            FolderPath = folderPath,
            Category = GetNamePart(nameParts, 0, "未分類"),
            SystemName = GetNamePart(nameParts, 1, "未知系統"),
            CustomerName = GetNamePart(nameParts, 2, "未知客戶"),
            StoreName = GetNamePart(nameParts, 3, "未知店別"),
            Version = nameParts.FirstOrDefault(part => part.StartsWith('V')) ?? "未知版本",
            OccurredAt = content.OccurredAt ?? timestamp,
            Subject = subject,
            From = content.From,
            PostedDate = content.PostedDate,
            Body = body,
            ContentHash = CreateContentHash(folderPath, subject, body, attachments),
            HasContentFile = content.HasContentFile,
            IsContentComplete = content.HasContentFile &&
                !string.IsNullOrWhiteSpace(content.Subject) &&
                content.PostedDate.HasValue &&
                !string.IsNullOrWhiteSpace(content.Body),
            Attachments = attachments
        };
    }

    private static ParsedContent ReadContent(string folderPath)
    {
        var contentPath = Path.Combine(folderPath, ContentFileName);
        if (!File.Exists(contentPath))
        {
            return new ParsedContent(false);
        }

        var text = ReadContentText(contentPath);
        var lines = text.ReplaceLineEndings("\n").Split('\n');
        var subject = ReadHeader(lines, "Subject:");
        var from = ReadHeader(lines, "From:");
        var postedDate = ParseDate(ReadHeader(lines, "PostedDate:"));
        var bodyIndex = Array.FindIndex(lines, line => line.Trim().Equals("Body:", StringComparison.OrdinalIgnoreCase));
        var body = bodyIndex >= 0
            ? string.Join(Environment.NewLine, lines.Skip(bodyIndex + 1)).Trim()
            : string.Empty;

        return new ParsedContent(true)
        {
            Subject = subject,
            From = from,
            PostedDate = postedDate,
            OccurredAt = ParseOccurredAt(body),
            Body = body
        };
    }

    private static string ReadHeader(string[] lines, string prefix)
    {
        var line = lines.FirstOrDefault(value => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return line is null ? string.Empty : line[prefix.Length..].Trim();
    }

    private static string ReadContentText(string contentPath)
    {
        var bytes = File.ReadAllBytes(contentPath);

        try
        {
            return Big5Encoding.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }

    private static Encoding CreateBig5Encoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(
            950,
            EncoderFallback.ExceptionFallback,
            DecoderFallback.ExceptionFallback);
    }

    private static DateTime? ParseOccurredAt(string body)
    {
        var match = OccurredAtRegex().Match(body);
        return match.Success ? ParseDate(match.Groups[1].Value.Trim()) : null;
    }

    private static DateTime? ParseFolderTimestamp(string folderName)
    {
        var match = FolderTimestampRegex().Match(folderName);
        if (!match.Success)
        {
            return null;
        }

        return DateTime.TryParseExact(
            match.Groups[1].Value + match.Groups[2].Value,
            "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result)
            ? result
            : null;
    }

    private static DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string[] formats =
        [
            "yyyy/M/d h:mm:ss tt",
            "yyyy/M/d tt h:mm:ss",
            "yyyy/MM/dd hh:mm:ss tt",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/M/d HH:mm:ss"
        ];

        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactResult))
        {
            return exactResult;
        }

        return DateTime.TryParse(value, CultureInfo.GetCultureInfo("zh-TW"), DateTimeStyles.None, out var result)
            ? result
            : null;
    }

    private static string GetNamePart(IReadOnlyList<string> parts, int index, string fallback)
    {
        return index < parts.Count && !string.IsNullOrWhiteSpace(parts[index])
            ? parts[index]
            : fallback;
    }

    private static string CreateStableId(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }

    private static string CreateContentHash(
        string folderPath,
        string subject,
        string body,
        IReadOnlyList<ErrorMailAttachment> attachments)
    {
        var builder = new StringBuilder();
        builder.AppendLine(folderPath);
        builder.AppendLine(subject);
        builder.AppendLine(body);

        foreach (var attachment in attachments)
        {
            builder.AppendLine($"{attachment.FileName}|{attachment.FullPath}|{attachment.Length}");
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
    }

    [GeneratedRegex(@"\[([^\]]*)\]")]
    private static partial Regex FolderBracketRegex();

    [GeneratedRegex(@"_(\d{8})_(\d{6})$")]
    private static partial Regex FolderTimestampRegex();

    [GeneratedRegex(@"發生時間：\s*([0-9/\-: ]+)")]
    private static partial Regex OccurredAtRegex();

    private sealed class ParsedContent(bool hasContentFile)
    {
        public bool HasContentFile { get; } = hasContentFile;

        public string Subject { get; init; } = string.Empty;

        public string From { get; init; } = string.Empty;

        public DateTime? PostedDate { get; init; }

        public DateTime? OccurredAt { get; init; }

        public string Body { get; init; } = string.Empty;
    }
}
