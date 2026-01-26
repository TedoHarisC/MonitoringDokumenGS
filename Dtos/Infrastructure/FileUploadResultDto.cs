public class FileUploadResult
{
    public string OriginalName { get; set; } = default!;
    public string StoredName { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
    public long Size { get; set; }
}
