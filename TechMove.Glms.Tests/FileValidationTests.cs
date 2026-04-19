using System.IO;
using System.Text;
using Moq;
using Microsoft.AspNetCore.Http;
using TechMove.Glms.Web.Services;
using Xunit;

namespace TechMove.Glms.Tests;

public class FileValidationTests
{
    private readonly FileValidationService _service = new FileValidationService();

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a mocked IFormFile bearing the given filename, content type,
    /// and raw byte content (so magic-bytes validation can be exercised).
    /// </summary>
    private static IFormFile MakeFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        return mockFile.Object;
    }

    private static byte[] PdfBytes(string extra = " body content")
        => Encoding.ASCII.GetBytes("%PDF-1.4" + extra);

    private static byte[] NonPdfBytes()
        => Encoding.ASCII.GetBytes("MZ\x90\x00 — this is an EXE header");

    // ── Happy-path ────────────────────────────────────────────────────────────

    [Fact]
    public void IsValidPdf_ValidPdfFile_ReturnsTrue()
    {
        var file = MakeFile("contract.pdf", "application/pdf", PdfBytes());
        Assert.True(_service.IsValidPdf(file));
    }

    // ── Extension / MIME failures ─────────────────────────────────────────────

    [Theory]
    [InlineData("image.jpg",     "image/jpeg")]
    [InlineData("program.exe",   "application/x-msdownload")]
    [InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void IsValidPdf_InvalidFileType_ReturnsFalse(string fileName, string contentType)
    {
        var file = MakeFile(fileName, contentType, NonPdfBytes());
        Assert.False(_service.IsValidPdf(file));
    }

    [Fact]
    public void IsValidPdf_PdfExtensionButWrongMime_ReturnsFalse()
    {
        // Extension claims PDF, but the browser's MIME type is wrong — should fail.
        var file = MakeFile("contract.pdf", "application/octet-stream", PdfBytes());
        Assert.False(_service.IsValidPdf(file));
    }

    // ── Magic bytes failures ──────────────────────────────────────────────────

    [Fact]
    public void IsValidPdf_CorrectExtensionAndMimeButWrongMagicBytes_ReturnsFalse()
    {
        // A renamed .exe pretending to be a PDF — magic bytes check must catch this.
        var file = MakeFile("disguised.pdf", "application/pdf", NonPdfBytes());
        Assert.False(_service.IsValidPdf(file));
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void IsValidPdf_NullFile_ReturnsFalse()
    {
        Assert.False(_service.IsValidPdf(null!));
    }

    [Fact]
    public void IsValidPdf_EmptyFile_ReturnsFalse()
    {
        // Zero-length file: no content at all, should fail immediately.
        var file = MakeFile("empty.pdf", "application/pdf", Array.Empty<byte>());
        Assert.False(_service.IsValidPdf(file));
    }

    [Fact]
    public void IsValidPdf_TruncatedFile_ReturnsFalse()
    {
        // File has only 3 bytes — cannot contain a valid %PDF- header (needs 5).
        var tinyContent = new byte[] { 0x25, 0x50, 0x44 }; // "%PD"
        var file = MakeFile("tiny.pdf", "application/pdf", tinyContent);
        Assert.False(_service.IsValidPdf(file));
    }
}
