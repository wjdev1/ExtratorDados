using ExtratorDados.Application.ExtratorDadosSalContINSS.Interfaces;
using ExtratorDados.WebMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExtratorDados.WebMVC.Controllers
{
    
    public class ExtratorDadosSalContINSSController : Controller
    {

        private readonly IExtraiDadosSalContrINSS _extraiDadosSalContrINSS;

        public ExtratorDadosSalContINSSController(IExtraiDadosSalContrINSS extraiDadosSalContrINSS)
        {
            _extraiDadosSalContrINSS = extraiDadosSalContrINSS;
        }

        public IActionResult Index()
        {
            var _extratorDadosSalContINSSViewModel = new ExtratorDadosSalContINSSViewModel();
            _extratorDadosSalContINSSViewModel.CaminhoExcelModelo = @"C:\Teste\ExtratorNovo\ModeloExcel\Exemplo - Novo Proc..xlsx";
            _extratorDadosSalContINSSViewModel.CaminhoPastaPDF = @"C:\Teste\ExtratorNovo\Folhas_pgtos_1-20220725T130033Z-001\Folhas_pgtos_1";

            return View(_extratorDadosSalContINSSViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Extrair([FromForm] ExtratorDadosSalContINSSViewModel extratorDadosSalContINSS)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", extratorDadosSalContINSS);

            await _extraiDadosSalContrINSS.GerarArquivoExcelModelo(extratorDadosSalContINSS.CaminhoPastaPDF, extratorDadosSalContINSS.CaminhoExcelModelo);

            return RedirectToAction("Index");
        }
    }
}
