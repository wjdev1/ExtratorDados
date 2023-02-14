using ExtratorDados.Application.ExtratorDadosSalContINSS.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ExtratorDados.Application.ExtratorDadosSalContINSSTests.Services
{
    public class ExtraiDadosSalContrINSSTests
    {
        private const string PATH_DEFAULT_PDF = @".\Resources\PDF_Padrao";

        [Fact]
        public void LerArquivosPDFPadrao()
        {
            var files = new ExtraiDadosSalContrINSS().LerArquivos(PATH_DEFAULT_PDF);
            var nomes = new List<string>();

            using (StreamWriter sw = new StreamWriter(PATH_DEFAULT_PDF + "nomes.csv"))
            {
                foreach (var file in files)
                {
                    var nome = file.Replace(PATH_DEFAULT_PDF, "");
                    var nomeSplit = nome.Split('.');
                    nomes.Add(nomeSplit[0]);
                }

                var nomesDisc = nomes.Distinct();

                foreach (var nome in nomesDisc)
                {
                    sw.WriteLine(nome);
                }

                sw.Close();
            }
         
            Assert.NotNull(files);
            Assert.True(files.Length == 4);
        }

        [Fact]
        public void ObterDadosDosArquivosPDFPadrao()
        {
            var files = new []
            {
                 @$"{PATH_DEFAULT_PDF}\Zenilto Vieira Izidoro.1.pdf",
                 @$"{PATH_DEFAULT_PDF}\Zenilto Vieira Izidoro.2.pdf",
                 @$"{PATH_DEFAULT_PDF}\Willian Silvestre de Oliveira.2.pdf",
                 @$"{PATH_DEFAULT_PDF}\Willian Silvestre de Oliveira.3.pdf",
            } ;

            var extrairDados = new ExtraiDadosSalContrINSS();
            extrairDados.ExtrairDadosColaboradores(files);

            Assert.NotNull(extrairDados.Colaboradores);
            Assert.True(extrairDados.Colaboradores.Count > 0);

        }

        [Fact]
        public void ExtrairDadosSalContrINSSExportandoArquivoBase()
        {
            var path = @"C:\Teste\ExtratorNovo\Folhas_pgtos_1-20220725T130033Z-001\Folhas_pgtos_1";
            new ExtraiDadosSalContrINSS().ExportarArquivoBase(path, @"c:\Teste\arquivoBase.xlsx"); ;
        }                           

        [Fact]
        public void ExtrairDadosSalContrINSSExportandoNoExcelModelo()
        {
            var path = @"C:\Teste\ExtratorNovo\Folhas_pgtos_1-20220725T130033Z-001\Folhas_pgtos_1";
            var excelModelo = @"C:\Teste\ExtratorNovo\ModeloExcel\Exemplo - Novo Proc..xlsx";
            new ExtraiDadosSalContrINSS().GerarArquivoExcelModelo(path, excelModelo);
        }
    }
}
