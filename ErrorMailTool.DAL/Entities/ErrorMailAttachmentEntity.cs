namespace ErrorMailTool.DAL.Entities;

public sealed class ErrorMailAttachmentEntity
{
    public int Id { get; set; }

    public required string ErrorMailId { get; set; }

    public required string FileName { get; set; }

    public required string FullPath { get; set; }

    public long Length { get; set; }

    public ErrorMailEntity? ErrorMail { get; set; }
}
