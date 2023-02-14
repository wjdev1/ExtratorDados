using ExtratorDados.Application.Services;
using ExtratorDadosConsoleApp.Helpers;
using System;
using System.IO;

namespace ExtratorDadosConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var processar = new ProcessarArquivoPDFAppService();
                processar._onNotificar += Notificando;

                processar.IniciarProcesso();
            }
            catch (Exception ex)
            {
                string Result = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(Result))
                    writer.Write($"ERRO: No carregamento da aplicação\n{ex.Message}");
            }
        }

        private static void Notificando(string notificacao, decimal? progressStatus, bool progressBar, bool error = false)
        {
            if (progressStatus != null && progressBar)
            {
                ConsoleUtility.WriteProgressBar((int)progressStatus, true);

                if (progressStatus == 100)
                {
                    Console.WriteLine();
                }
            }
            else if (progressStatus != null)
            {
                ConsoleUtility.WriteProgress((int)progressStatus, true);
            }
            else
                Console.WriteLine($"{notificacao}");
        }

    }
}
