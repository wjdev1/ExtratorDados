using System;
using System.IO;
using System.Text;
using static ExtratorDados.Domain.Services.BaseService;

namespace ExtratorDados.CrossCutting.Write.TXT.Services
{
    public class WriteTXTServices
    {
        public event OnNotificador _onNotificar;

        public void Write(StringBuilder texto, string path)
        {
            try
            {
                using (var writer = new StreamWriter(path))
                    writer.Write(texto.ToString());
            }
            catch (System.Exception ex)
            {
                string erro = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(erro))
                    writer.Write($"ERRO: Na escrita txt\n{ex.Message}");

                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }
        }
    }
}
