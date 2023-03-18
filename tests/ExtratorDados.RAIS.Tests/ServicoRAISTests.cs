using ExtratorDados.Application.RAIS;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace ExtratorDados.RAIS.Tests
{
    public class ServicoRAISTests
    {
        [Fact]
        public async Task DeveSplitarArquivoPDF()
        {
            //Arrange
            string[] files = { @"C:\Workana\ExtratorDados\tests\resources\Processo_0001340-97 - Somente folhas necessarias.pdf" };
            string destino = @"C:\Workana\ExtratorDados\tests\resources\EXTRATOS_DEPOSITOS\";
            var _serviceSPLITPDF = new CrossCutting.PDF.Services.SplitPDFService();

            //Action
            await _serviceSPLITPDF.Split(files, destino, 4089, 8384, false);
        }

        [Fact]
        public void LerColaboradorNoArquivoRAIS()
        {
            //Arrange
            var path = @"E:\Development\Workana\temp\RAIS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path).GetFiles();

            //files = files.Where(x => x.Name == "1290_a17397a3-b07a-4d59-9f3f-65731ff016d3.pdf").ToArray();

            //Action
            foreach (var file in files)
            {
                var dados = service.ReadPDF(file.FullName);
                service.ManipularDadosRAIS(dados);
                var dados2 = service.ReadPDF_UTF8(file.FullName);
                service.ManipularDadosRAISRemuneracao(dados2);
            }

            var _repositoryRemuneracao = new CrossCutting.Dados.RAIS.RAISRepository<Domain.Rais.Entities.Remuneracao>();

            foreach (var colaborador in service.Colaboradores)
            {
                //var _repositoryColaborador = new CrossCutting.Dados.RAIS.RAISRepository<Domain.Rais.Entities.Colaborador>();

                //_repositoryColaborador.Add(colaborador);

                colaborador.Remuneracoes.ForEach(rem =>
                {
                    _repositoryRemuneracao.Add(rem);
                });
            }
        }

        [Fact]
        public void LerColaboradorNoArquivoRAISObterDecimoTerceiroEAviso()
        {
            //Arrange
            var path = @"E:\Development\Workana\temp\RAIS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path).GetFiles();

            //Action
            foreach (var file in files)
            {
                var dados = service.ReadPDF(file.FullName);
                service.ManipularDadosRAISDecimoTerceiroEAviso(dados.ToString());
            }
        }

        [Fact]
        public void LerCompNaoLocalizadas()
        {
            //Arrange
            var path = @"E:\Development\Workana\temp\COMP_NAO_LOCALIZADAS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path).GetFiles().OrderBy(o => int.Parse(Regex.Match(o.FullName, @"\d+").Value)).ToList();

            //files = files.Where(x => x.FullName.EndsWith("302.pdf") || x.FullName.EndsWith("303.pdf")).ToList();

            FileInfo _fileTemp = null;

            //Action
            try
            {
                foreach (var file in files)
                {
                    _fileTemp = file;
                    //Thread.Sleep(1000);
                    var dados = service.ReadPDF_ToString(file.FullName);
                    service.ManipularCompNaoLocalizadas(dados, file);
                }
            }
            catch (System.Exception ex)
            {
                var arquivo = _fileTemp;
            }
        }

        [Fact]
        public void LerExtratoPagamento()
        {
            //Arrange
            var path = @"E:\Development\Workana\temp\EXTRATOS_DEPOSITOS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path)
                .GetFiles("*.pdf")
                .OrderBy(o => int.Parse(Regex.Match(o.FullName, @"\d+").Value))
                .ToList();

            //Action
            foreach (var file in files)
            {
                var dados = service.ReadPDF(file.FullName);

                var size = dados.ToString().Split('\n')
                    .Select(s => s).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                if (size.Length > 5)
                    service.ManipularExtratoFGTS(dados, file);
            }
        }

        [Fact]
        public void GerarTXTPaginasComImagem()
        {
            //Arrange
            var path = @"C:\Workana\ExtratorDados\tests\resources\EXTRATOS_DEPOSITOS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path).GetFiles().OrderBy(o => int.Parse(Regex.Match(o.FullName, @"\d+").Value)).ToList();

            int count = 0;

            using (StreamWriter sw = new StreamWriter(@"C:\Workana\ExtratorDados\tests\resources\EXTRATOS_DEPOSITOS\paginas_imagem.txt"))
            {
                //Action
                foreach (var file in files)
                {
                    var dados = service.ReadPDF(file.FullName);

                    var size = dados.ToString().Split('\n').Select(s => s).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                    if (size.Length < 5)
                    {
                        sw.WriteLine($"página: {file.Name.Replace(".pdf", "")}");
                        count++;

                        if (size.Length > 2)
                        {

                        }
                    }
                }

                sw.WriteLine($"total:{count}");
                sw.Close();
            }
        }

        [Fact]
        public async void GerarArquivoCSV()
        {
            await new RAISService().GerarArquivoCSV();
        }
    }
}

