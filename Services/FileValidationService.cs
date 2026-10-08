namespace TechMove.Services;

public class FileValidationService
{
    public const long MaxFileSize = 10 * 1024 * 1024;

    public void ValidatePdf(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidDataException("A signed agreement PDF is required.");

        if (file.Length > MaxFileSize)
            throw new InvalidDataException("The signed agreement must be 10 MB or smaller.");

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Only PDF files are allowed for signed agreements.");

        if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The signed agreement must have PDF content type.");
    }
}
