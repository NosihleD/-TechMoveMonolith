using Microsoft.AspNetCore.Http;
using TechMove.Services;

namespace TechMove.Tests;

public class FileValidationServiceTests
{
    [Fact]
    public void ValidatePdf_RejectsExe()
    {
        var service = new FileValidationService();
        var file = CreateFile("malware.exe", "application/octet-stream", "not pdf");
        var ex = Assert.Throws<InvalidDataException>(() => service.ValidatePdf(file));
        Assert.Contains("Only PDF", ex.Message);
    }

    [Fact]
    public void ValidatePdf_RejectsWrongContentType()
    {
        var service = new FileValidationService();
        var file = CreateFile("agreement.pdf", "application/octet-stream", "fake");
        Assert.Throws<InvalidDataException>(() => service.ValidatePdf(file));
    }

    [Fact]
    public void ValidatePdf_AcceptsPdf()
    {
        var service = new FileValidationService();
        var file = CreateFile("agreement.pdf", "application/pdf", "%PDF-1.7");
        service.ValidatePdf(file);
    }

    private static IFormFile CreateFile(string name, string contentType, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "signedAgreement", name) { Headers = new HeaderDictionary(), ContentType = contentType };
    }
}
