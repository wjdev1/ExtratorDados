using ExtratorDados.CrossCutting.Read.TXT.Services;
using ExtratorDados.CrossCutting.Write.CSV.Services;
using System;
using System.Diagnostics;
using System.IO;
using static ExtratorDados.Domain.Services.BaseService;

namespace ExtratorDados.Application.Services
{
    public class ProcessarArquivoPDFAppService
    {
        public event OnNotificador _onNotificar;

        public void IniciarProcesso(string _arquivoPDF = null, string _destino = null, bool _usarConfig = false)
        {
            try
            {
                var watch = Stopwatch.StartNew();

                _destino = _destino + "\\ExtratorDados_" + DateTime.Now.ToString("ddMMyyyyhhmmss");
                Directory.CreateDirectory(_destino);

                _onNotificar.Invoke("Iniciando", null, false);

                var _leituraPDFService = new CrossCutting.Read.PDF.ExtratorDados3500.ReadPDFService();
                _leituraPDFService._onNotificar += _onNotificar;
                _leituraPDFService.ReadPDFTOWriteTXTTemp(_arquivoPDF);

                var _leituraTXTService = new ReadTXTService();
                _leituraTXTService._onNotificar += _onNotificar;
                var colaboradores = _leituraTXTService.LerTXT();

                var _exportarLocalService = new CrossCutting.Write.Excel.ExtratorDados3500.ExportarArquivoExcel();
                _exportarLocalService._onNotificar += _onNotificar;
                _exportarLocalService.GerarXLSX(colaboradores, _destino);

                var writeCSVService = new WriteCSVService(colaboradores, _usarConfig);
                writeCSVService._onNotificar += _onNotificar;
                writeCSVService.ExportarCSVLocalmente(_destino);

                watch.Stop();

                Console.WriteLine($"Fim do processo: {watch.Elapsed.TotalSeconds} segundos");

            }
            catch (Exception ex)
            {
                string Result = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(Result))
                    writer.Write($"ERRO: No carregamento da aplicação\n{ex.Message}");

                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }
        }

    }
}
