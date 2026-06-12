namespace ErrorMailTool.BLL.Models;

public sealed class ErrorMailAttachmentDto
{
    public required string FileName { get; init; }

    public required string FullPath { get; init; }

    public long Length { get; init; }

    public string DisplaySize => Length >= 1024 * 1024
        ? $"{Length / 1024d / 1024d:N1} MB"
        : $"{Length / 1024d:N1} KB";
}
