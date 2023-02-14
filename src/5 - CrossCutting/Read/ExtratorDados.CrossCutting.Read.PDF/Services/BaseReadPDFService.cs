using ExtratorDados.CrossCutting.Write.TXT.Services;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Text;
using static ExtratorDados.Domain.Services.BaseService;

namespace ExtratorDados.CrossCutting.Read.PDF.Services
{
    public abstract class BaseReadPDFService
    {
        /// <summary>
        /// Event notificador
        /// </summary>
        public event OnNotificador _onNotificar;
        /// <summary>
        /// Ler Arquivo PDF
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StringBuilder ReadPDF(string path)
        {
            var documentText = new StringBuilder();
            int qtdPaginasLidas = 0;

            try
            {
                if (File.Exists(path))
                {

                    using (PdfReader reader = new PdfReader(path))
                    {
                        var codePages = CodePagesEncodingProvider.Instance;
                        Encoding.RegisterProvider(codePages);

                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            var line = PdfTextExtractor.GetTextFromPage(reader, i);
                            documentText.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                            qtdPaginasLidas++;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string erro = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(erro))
                    writer.Write($"ERRO: Na leitura do arquivo pdf\nPág:{qtdPaginasLidas}\n{ex.Message}");
            }

            return documentText;
        }
        public string ReadPDF_UTF8(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                var codePages = CodePagesEncodingProvider.Instance;
                Encoding.RegisterProvider(codePages);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.Default, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }

        /// <summary>
        /// Ler Arquivo PDF notificando leitura por evento
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StringBuilder ReadPDFOnNotify(string path)
        {
            _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.efetuando_leitura_de_arquivo__0_, "pdf"), null, false);

            var documentText = new StringBuilder();
            int qtdPaginasLidas = 0;

            try
            {
                if (File.Exists(path))
                {

                    using (PdfReader reader = new PdfReader(path))
                    {
                        _onNotificar?.Invoke("efetuando leitura pdf", 0.1m, false);

                        var codePages = CodePagesEncodingProvider.Instance;
                        Encoding.RegisterProvider(codePages);

                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            documentText.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                            qtdPaginasLidas++;

                            _onNotificar.Invoke("efetuando leitura pdf", Math.Round(qtdPaginasLidas / Convert.ToDecimal(reader.NumberOfPages), 6), true);
                        }

                        if (string.IsNullOrEmpty(documentText.ToString()))
                            _onNotificar.Invoke($"Ocorreu um erro na leitura do arquivo pdf, verifique se o arquivo existe no local informado", null, false, error: true);
                    }
                }

            }
            catch (Exception ex)
            {
                string erro = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(erro))
                    writer.Write($"ERRO: Na leitura do arquivo pdf\nPág:{qtdPaginasLidas}\n{ex.Message}");

                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }

            return documentText;
        }

        /// <summary>
        /// Ler Arquivo PDF, salva em um arquivo TXT, retorna caminho do arquivo salvo em .txt
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadPDFTOWriteTXTTemp(string path)
        {
            _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.efetuando_leitura_de_arquivo__0_, "pdf"), null, false);

            int qtdPaginasLidas = 0;

            try
            {
                if (File.Exists(path))
                {
                    var documentText = new StringBuilder();

                    using (PdfReader reader = new PdfReader(path))
                    {
                        _onNotificar?.Invoke("efetuando leitura pdf", 0.1m, false);

                        var codePages = CodePagesEncodingProvider.Instance;
                        Encoding.RegisterProvider(codePages);

                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            documentText.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                            qtdPaginasLidas++;

                            _onNotificar.Invoke("efetuando leitura pdf", Math.Round(qtdPaginasLidas / Convert.ToDecimal(reader.NumberOfPages), 6), true);
                        }

                        if (string.IsNullOrEmpty(documentText.ToString()))
                            _onNotificar.Invoke($"Ocorreu um erro na leitura do arquivo pdf, verifique se o arquivo existe no local informado", null, false, error: true);
                        //throw new Exception("Ocorreu um erro na leitura do arquivo pdf, verifique se o arquivo existe no local informado");

                        if (File.Exists($"{Environment.CurrentDirectory}\\result.txt"))
                            File.Delete($"{Environment.CurrentDirectory}\\result.txt");

                        _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.criando_arquivo__0_, "temp"), null, false);

                        new WriteTXTServices().Write(documentText, $"{Environment.CurrentDirectory}\\result.txt");
                    }
                }

            }
            catch (Exception ex)
            {
                string erro = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(erro))
                    writer.Write($"ERRO: Na leitura do arquivo pdf\nPág:{qtdPaginasLidas}\n{ex.Message}");

                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }

            return $"{Environment.CurrentDirectory}\\result.txt";
        }

        /// <summary>
        /// Ler Arquivo PDF, salva em um arquivo TXT, retorna caminho do arquivo salvo em .txt
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadPDFTOWriteTXTTemp(string path, int? pagInicial = null, int? pagFinal = null)
        {
            _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.efetuando_leitura_de_arquivo__0_, "pdf"), null, false);

            int qtdPaginasLidas = 0;

            try
            {
                if (File.Exists(path))
                {
                    var documentText = new StringBuilder();

                    using (PdfReader reader = new PdfReader(path))
                    {
                        _onNotificar?.Invoke("efetuando leitura pdf", 0.1m, false);

                        var codePages = CodePagesEncodingProvider.Instance;
                        Encoding.RegisterProvider(codePages);

                        pagInicial = pagInicial == null ? 1 : pagInicial;
                        pagFinal = pagFinal == null ? 1 : reader.NumberOfPages;

                        for (int pag = pagInicial.Value; pag <= pagFinal.Value; pag++)
                        {
                            documentText.Append(PdfTextExtractor.GetTextFromPage(reader, pag));
                            qtdPaginasLidas++;

                            _onNotificar.Invoke("efetuando leitura pdf", Math.Round(qtdPaginasLidas / Convert.ToDecimal(reader.NumberOfPages), 6), true);
                        }

                        if (string.IsNullOrEmpty(documentText.ToString()))
                            _onNotificar.Invoke($"Ocorreu um erro na leitura do arquivo pdf, verifique se o arquivo existe no local informado", null, false, error: true);
                        //throw new Exception("Ocorreu um erro na leitura do arquivo pdf, verifique se o arquivo existe no local informado");

                        if (File.Exists($"{Environment.CurrentDirectory}\\result.txt"))
                            File.Delete($"{Environment.CurrentDirectory}\\result.txt");

                        _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.criando_arquivo__0_, "temp"), null, false);

                        new WriteTXTServices().Write(documentText, $"{Environment.CurrentDirectory}\\result.txt");
                    }
                }

            }
            catch (Exception ex)
            {
                string erro = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(erro))
                    writer.Write($"ERRO: Na leitura do arquivo pdf\nPág:{qtdPaginasLidas}\n{ex.Message}");

                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }

            return $"{Environment.CurrentDirectory}\\result.txt";
        }
    }
}
