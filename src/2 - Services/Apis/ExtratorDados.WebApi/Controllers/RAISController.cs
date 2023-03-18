using CsvHelper;
using CsvHelper.Configuration;
using ExtratorDados.Application.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ExtratorDados.WebApi.Controllers
{
    [Route("api/v1/rais")]
    public class RAISController : Controller
    {
        [HttpGet("export-colaboradores-nao-pago-fgts")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ObterColaboradoresNaoPagoFGTS()
        {
            var colaboradores = await new Application.RAIS.RAISService().ArquivoCompletoRAIs();

            if (colaboradores == null) return NotFound();

            var cc = new CsvConfiguration(new System.Globalization.CultureInfo("en-US"));
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream: ms, encoding: new UTF8Encoding(true)))
                {
                    using (var cw = new CsvWriter(sw, cc))
                    {
                        cw.WriteRecords(colaboradores);
                    }// The stream gets flushed here.
                    return File(ms.ToArray(), "text/csv", $"export_{DateTime.UtcNow.Ticks}.csv");
                }
            }
        }

        [HttpPost("gerar-csv")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GerarCSVRAIS([FromBody] ArquivoDto arquivoDto)
        {
            await new Application.RAIS.RAISService(arquivoDto.FolderDestino).GerarArquivoCSV();

            return Ok();
        }
    }
}
