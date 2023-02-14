using ExtratorDados.Domain.Services;
using NUnit.Framework;

namespace ExtratorDados.CrossCutting.Read.PDFTests.Services
{
    public class ReadPDFServiceTests
    {
        [Test]
        public void ExtrairDadosPDF_ExtratorDadosSalContINSS()
        {
            var path = @"C:\Workana\ExtratorDados\tests\resources\Processo_0001340-97 - Somente folhas necessarias.pdf";
            var readPDFService = new PDF.ExtratorDadosSalContINSS.ReadPDFService();
            readPDFService._onNotificar += ReadPDFService__onNotificar;
            var dados = readPDFService.ReadPDF(path).ToString();

            new ProcessarDadosDoArquivo().LerDados(dados);
        }

        [Test]
        public void ExtrairDadosPDFRAIS()
        {
            var path = @"C:\Workana\ExtratorDados\tests\resources\Processo_0001340-97 - Somente folhas necessarias.pdf";
            var readPDFService = new PDF.ExtratorDados3500.ReadPDFService();
            readPDFService._onNotificar += ReadPDFService__onNotificar;
            var dados = readPDFService.ReadPDF(path).ToString();

            new ProcessarDadosDoArquivo().LerDados(dados);
        }

        private void ReadPDFService__onNotificar(string mensagem, decimal? progress, bool progressBar, bool error = false)
        {

        }
    }
}
