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
            var path = @"C:\Workana\ExtratorDados\tests\resources\RAIS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path).GetFiles();

            //Action
            foreach (var file in files)
            {
                var dados = service.ReadPDF(file.FullName);
                service.ManipularDadosRAIS(dados);
                var dados2 = service.ReadPDF_UTF8(file.FullName);
                service.ManipularDadosRAISRemuneracao(dados2);
            }

        //    var _repositoryRemuneracao = new CrossCutting.Dados.RAIS.RAISRepository<Domain.Rais.Entities.Remuneracao>();

        //    foreach (var colaborador in service.Colaboradores)
        //    {
        //        var _repositoryColaborador = new CrossCutting.Dados.RAIS.RAISRepository<Domain.Rais.Entities.Colaborador>();
                
        //        _repositoryColaborador.Add(colaborador);

        //        colaborador.Remuneracoes.ForEach(rem => 
        //        {
        //            _repositoryRemuneracao.Add(rem);    
        //        });
        //    }
        }

        [Fact]
        public void LerColaboradorNoArquivoRAISObterDecimoTerceiroEAviso()
        {
            //Arrange
            var path = @"C:\Workana\ExtratorDados\tests\resources\RAIS\";
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
            var path = @"C:\Workana\ExtratorDados\tests\resources\COMP_NAO_LOCALIZADAS\";
            var service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();

            var files = new DirectoryInfo(path).GetFiles().OrderBy(o => int.Parse(Regex.Match(o.FullName, @"\d+").Value)).ToList();

            //Action
            foreach (var file in files)
            {
                var dados = service.ReadPDF(file.FullName);
                service.ManipularCompNaoLocalizadas(dados);
            }

            //service.CompetenciasNaoLocalizadas.ForEach(comp => 
            //{
            //    var _repository = new CrossCutting.Dados.RAIS.RAISRepository<Domain.Rais.Entities.CompNaoLocalizada>();
            //    _repository.Add(comp);
            //});

            //service.ExtratosFGTS.ForEach(extrato => 
            //{
            //    var _repository = new CrossCutting.Dados.RAIS.RAISRepository<Domain.Rais.Entities.ExtratoFGTS>();
            //    _repository.Add(extrato);   
            //});
        }

        [Fact]
        public void LerExtratoPagamento()
        {
            //Arrange
            var path = @"C:\Workana\ExtratorDados\tests\resources\EXTRATOS_DEPOSITOS\";
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
        public async void ServicoRAIS()
        {
            var pathRAIS = @"C:\Workana\ExtratorDados\tests\resources\RAIS\";
            var pathExtratos = @"C:\Workana\ExtratorDados\tests\resources\COMP_NAO_LOCALIZADAS\";

            var service = new Application.RAIS.RAISService();
            await service.Processar(pathRAIS, pathExtratos);

            var arquivo = new Domain.Rais.Entities.ArquivoCompletoRAIS();
        }

        private void ReadPDFService__onNotificar(string mensagem, decimal? progress, bool progressBar, bool error = false)
        {

        }
    }
}

