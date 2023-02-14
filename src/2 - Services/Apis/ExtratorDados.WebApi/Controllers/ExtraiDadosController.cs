using ExtratorDados.Application.ExtratorDadosSalContINSS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExtratorDados.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/extrai-dados")]
    public class ExtraiDadosController : Controller
    {
        private readonly IExtraiDadosSalContrINSS _extraiDadosSalContrINSS;

        public ExtraiDadosController(IExtraiDadosSalContrINSS extraiDadosSalContrINSS)
        {
            _extraiDadosSalContrINSS = extraiDadosSalContrINSS;
        }

        [HttpPost]
        public async Task<IActionResult> ExtrairDadosSalContINSS(string pathFilesPDF, string excelModelo)
        {
            await _extraiDadosSalContrINSS.GerarArquivoExcelModelo(pathFilesPDF, excelModelo);

            return Ok();
        }
    }
}
