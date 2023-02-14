using Xunit;

namespace ExtratorDados.CrossCutting.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void DeveLerPDF_RAIS()
        {
            //Arrange
            var path = "";
            var _servicePDF_RAIS = new Read.PDF.Rais.RAISLeituraPDFService();
            _servicePDF_RAIS.ReadPDFTOWriteTXTTemp(path);
        }
    }
}
