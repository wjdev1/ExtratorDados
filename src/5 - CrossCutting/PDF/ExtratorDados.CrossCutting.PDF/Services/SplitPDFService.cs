using iText.Kernel.Pdf;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExtratorDados.CrossCutting.PDF.Services
{
    public class SplitPDFService : BasePDFService
    {
        #region Construtor
        public SplitPDFService() : base("") { }
        #endregion

        #region Metodos
        public async Task Split(string[] files, string destino)
        {
            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    using (var pdfDoc = new PdfDocument(new PdfReader(file)))
                    {
                        // Loop pages in the given document.
                        int numberOfPages = pdfDoc.GetNumberOfPages();
                        for (int i = 0; i < numberOfPages; i++)
                        {
                            // Determine destination file path for current page.
                            int currentPageNumber = i + 1;
                            string fileName = $"{currentPageNumber}_{Guid.NewGuid()}.pdf";
                            string pageFilePath = Path.Combine(destino, fileName);

                            // Write current page to disk.
                            using (PdfWriter writer = new PdfWriter(pageFilePath))
                            {
                                using (var pdf = new PdfDocument(writer))
                                {
                                    pdfDoc.CopyPagesTo(pageFrom: currentPageNumber, pageTo: currentPageNumber, toDocument: pdf, insertBeforePage: 1);
                                }
                            }
                        }
                    }
                }
            });
        }

        public async Task Split(string[] files, string destino, int pagInicial, int pagFinal, bool nameGuid = true)
        {
            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    using (var pdfDoc = new PdfDocument(new PdfReader(file)))
                    {
                        // Loop pages in the given document.
                        int numberOfPages = pdfDoc.GetNumberOfPages();
                        for (int page = 0; page < numberOfPages; page++)
                        {
                            if (page < pagInicial || page > pagFinal) continue;

                            // Determine destination file path for current page.
                            int currentPageNumber = page + 1;
                            string fileName = nameGuid ? $"{currentPageNumber}_{Guid.NewGuid()}.pdf" : $"{currentPageNumber}.pdf";
                            string pageFilePath = Path.Combine(destino, fileName);

                            // Write current page to disk.
                            using (PdfWriter writer = new PdfWriter(pageFilePath))
                            {
                                using (var pdf = new PdfDocument(writer))
                                {
                                    pdfDoc.CopyPagesTo(pageFrom: currentPageNumber, pageTo: currentPageNumber, toDocument: pdf, insertBeforePage: 1);
                                }
                            }
                        }
                    }
                }
            });
        }
        #endregion
    }
}
