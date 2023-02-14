using System.Threading.Tasks;

namespace ExtratorDados.Application.ExtratorDadosSalContINSS.Interfaces
{
    public interface IExtraiDadosSalContrINSS
    {
        Task GerarArquivoExcelModelo(string path, string pathPlanilhaModelo);
    }
}
