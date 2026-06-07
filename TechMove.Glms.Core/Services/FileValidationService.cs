using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace TechMove.Glms.Web.Services
{
    public class FileValidationService
    {
        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                return false;
            }

            string contentType = file.ContentType.ToLowerInvariant();
            if (contentType != "application/pdf")
            {
                return false;
            }

            byte[] PdfMagicBytes = Encoding.ASCII.GetBytes("%PDF-");
            bool isValid = true;

            using (Stream stream = file.OpenReadStream())
            {
                byte[] header = new byte[PdfMagicBytes.Length];
                int bytesRead = stream.Read(header, 0, header.Length);

                if (bytesRead < PdfMagicBytes.Length)
                {
                    isValid = false;
                }
                else
                {
                    for (int i = 0; i < PdfMagicBytes.Length; i++)
                    {
                        if (header[i] != PdfMagicBytes[i])
                        {
                            isValid = false;
                        }
                    }
                }
            }

            return isValid;
        }
    }
}
