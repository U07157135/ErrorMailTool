namespace ErrorMailTool.DAL.Models;

public sealed class ErrorMailAttachment
{
    public required string FileName { get; init; }

    public required string FullPath { get; init; }

    public long Length { get; init; }
}
