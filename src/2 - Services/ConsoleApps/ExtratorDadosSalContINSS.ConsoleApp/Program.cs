using ExtratorDados.Application.ExtratorDadosSalContINSS.Interfaces;
using ExtratorDados.Application.ExtratorDadosSalContINSS.Services;

namespace ExtratorDadosSalContINSS.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            IExtraiDadosSalContrINSS _extraiDadosSalContrINSS = new ExtraiDadosSalContrINSS();
            var path = @"C:\Teste\ExtratorNovo\Folhas_pgtos_1-20220725T130033Z-001\Folhas_pgtos_1";
            var excelModelo = @"C:\Teste\ExtratorNovo\ModeloExcel\Exemplo - Novo Proc..xlsx";

            try
            {
                _extraiDadosSalContrINSS.GerarArquivoExcelModelo(path, excelModelo);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }
    }
}
