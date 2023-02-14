using ExtratorDados.Application.ExtratorDadosSalContINSS.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtratorDados.Application.ExtratorDadosSalContINSS.Services
{
    public class ExtraiDadosSalContrINSS : IExtraiDadosSalContrINSS
    {
        private List<Models.Colaborador> _colaboradores = new List<Models.Colaborador>();

        public IReadOnlyCollection<Models.Colaborador> Colaboradores { get { return _colaboradores; } }

        public async Task GerarArquivoExcelModelo(string path, string pathPlanilhaModelo)
        {
            await Task.Factory.StartNew(() => 
            {
                var files = LerArquivos(path);
                ExtrairDadosColaboradores(files);

                var colaboradores = InserirDadosDOsColaboradores();

                InserirDadosNoExcel(pathPlanilhaModelo, colaboradores);
            });
        }

        public string[] LerArquivos(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Caminho inválido!");

            try
            {
                return Directory.GetFiles(path, "*.pdf", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ExtrairDadosColaboradores(string[] files)
        {
            if (files == null || files.Length == 0)
                throw new ArgumentException("Não foi informado nenhum arquivo PDF!");
            try
            {
                var pdfService = new CrossCutting.Read.PDF.ExtratorDadosSalContINSS.ReadPDFService();

                foreach (var file in files)
                {
                    var dados = pdfService.ReadPDF(file).ToString();
                    ObterDados(dados);
                }                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void ObterDados(string texto)
        {
            try
            {
                if (texto.Contains("Assinatura do Funcionário"))
                    LayoutComAssinaturaFuncionario(texto);
                else
                    LayoutSemAssinatura(texto);
            }
            catch (Exception ex)
            {

            }
        }

        private void LayoutComAssinaturaFuncionario(string texto)
        {
            try
            {
                var trechoMensal = texto.Trim().Replace("()", "").Split("Assinatura do Funcionário");

                foreach (var trecho in trechoMensal)
                {
                    if (string.IsNullOrWhiteSpace(trecho))
                        continue;

                    var colaborador = new Models.Colaborador();

                    var nomeFuncionario = Regex.Match(trecho, @"(?i)\d\s\/\s\d\s(?<nomeFuncionario>\w.*)");
                    var funcaoFuncionario = Regex.Match(trecho, @"(?i)Fun..o:\s(?<funcaoColaborador>\w.*)(\n)?Data");
                    var mesAno = Regex.Match(trecho, @"(?i)RECIBO DE PAGAMENTO\s\-\s(?<mesAno>\w.*\/\d{4})");
                    var salContrINSS = Regex.Match(trecho, @"(?i)IRF\n\s(?<salContrINSS>\w.*)");

                    if (nomeFuncionario.Groups["nomeFuncionario"].Success &&
                        nomeFuncionario.Success &&
                        !string.IsNullOrEmpty(nomeFuncionario.Value))
                    {
                        colaborador.NomeFuncionario = nomeFuncionario.Groups["nomeFuncionario"].Value;
                    }
                    else
                    {
                        nomeFuncionario = Regex.Match(trecho, @"(?i)\d\s(\/\s)?\d\s(?<nomeFuncionario>\w.*)\n\/\nFun..o");
                        var nomeFuncionario2 = Regex.Match(trecho, @"(?i)\d\s(\/\s)(?<nomeFuncionario>\w.*)\nFun..o");

                        if (nomeFuncionario.Groups["nomeFuncionario"].Success &&
                        nomeFuncionario.Success &&
                        !string.IsNullOrEmpty(nomeFuncionario.Value))
                        {
                            colaborador.NomeFuncionario = nomeFuncionario.Groups["nomeFuncionario"].Value;
                        }
                        else if (nomeFuncionario2.Groups["nomeFuncionario"].Success &&
                        nomeFuncionario2.Success &&
                        !string.IsNullOrEmpty(nomeFuncionario2.Value))
                        {
                            colaborador.NomeFuncionario = nomeFuncionario2.Groups["nomeFuncionario"].Value;
                        }
                    }

                    if (funcaoFuncionario.Groups["funcaoColaborador"].Success &&
                        funcaoFuncionario.Success &&
                        !string.IsNullOrEmpty(funcaoFuncionario.Value))
                    {
                        colaborador.FuncaoFuncionario = funcaoFuncionario.Groups["funcaoColaborador"].Value;
                    }
                    else
                    {
                        funcaoFuncionario = Regex.Match(trecho, @"(?i)(?<funcaoColaborador>\w.*)\s\d{2}\/\d{2}\/\d{4}\nFun..o");

                        if (funcaoFuncionario.Groups["funcaoColaborador"].Success &&
                        funcaoFuncionario.Success &&
                        !string.IsNullOrEmpty(funcaoFuncionario.Value))
                        {
                            colaborador.FuncaoFuncionario = funcaoFuncionario.Groups["funcaoColaborador"].Value;
                        }
                    }

                    if (mesAno.Groups["mesAno"].Success &&
                        mesAno.Success &&
                        !string.IsNullOrEmpty(mesAno.Value))
                    {
                        colaborador.MesAno = Convert.ToDateTime(mesAno.Groups["mesAno"].Value.Replace("MARCO", "MARÇO"));
                    }
                    else
                    {
                        mesAno = Regex.Match(trecho, @"(?i)\n(?<mesAno>\w.*\/\d{4})\nRECIBO DE PAGAMENTO");
                        var mesAno2 = Regex.Match(trecho, @"(?i)RECIBO DE PAGAMENTO\s\-\s\n(?<mesAno>\w.*\/\d{4})");

                        if (mesAno.Success &&
                        !string.IsNullOrEmpty(mesAno.Value))
                        {
                            if (mesAno.Groups["mesAno"].Success)
                                colaborador.MesAno = Convert.ToDateTime(mesAno.Groups["mesAno"].Value.Replace("MARCO", "MARÇO"));
                        }
                        else if (mesAno2.Success &&
                        !string.IsNullOrEmpty(mesAno2.Value))
                        {
                            colaborador.MesAno = Convert.ToDateTime(mesAno2.Groups["mesAno"].Value.Replace("MARCO", "MARÇO"));
                        }
                    }

                    if (salContrINSS.Groups["salContrINSS"].Success &&
                        salContrINSS.Success &&
                        !string.IsNullOrEmpty(salContrINSS.Value))
                    {
                        var valorSalContrINSS = salContrINSS.Groups["salContrINSS"].Value.Trim().Split("R$");

                        if (valorSalContrINSS.Length == 2)
                        {
                            salContrINSS = Regex.Match(trecho, @"(?i)IRF\n\s(?<salContrINSS>\w.*\n\s\w.*)");
                            valorSalContrINSS = salContrINSS.Groups["salContrINSS"].Value.Trim().Split("R$");
                        }

                        colaborador.SalContrINSS = Convert.ToDecimal(valorSalContrINSS[2].Trim());
                    }

                    if (!colaborador.IsValid())
                        throw new Exception($"Não foi possível obter dados do funcionário\ndados:{trecho}");

                    if (!_colaboradores.Any(x => x.NomeFuncionario.ToLower() == colaborador.NomeFuncionario.ToLower() && x.MesAno == colaborador.MesAno))
                        _colaboradores.Add(colaborador);
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void LayoutSemAssinatura(string texto)
        {
            try
            {
                var trechoMensal = texto.Trim().Replace("()", "").Split("RECIBO DE");

                foreach (var trecho in trechoMensal)
                {
                    if (string.IsNullOrWhiteSpace(trecho))
                        continue;

                    var colaborador = new Models.Colaborador();

                    var nomeFuncionario = Regex.Match(trecho, @"(?i)\d\s\/\s\d\s(?<nomeFuncionario>\w.*)");
                    var funcaoFuncionario = Regex.Match(trecho, @"(?i)Fun..o:\s(?<funcaoColaborador>\w.*)(\n)?Data");
                    var mesAno = Regex.Match(trecho, @"(?i)PAGAMENTO\s\-\s(?<mesAno>\w.*\/\d{4})");
                    var salContrINSS = Regex.Match(trecho, @"(?i)IRF\n\s(?<salContrINSS>\w.*)");

                    if (nomeFuncionario.Groups["nomeFuncionario"].Success &&
                        nomeFuncionario.Success &&
                        !string.IsNullOrEmpty(nomeFuncionario.Value))
                    {
                        colaborador.NomeFuncionario = nomeFuncionario.Groups["nomeFuncionario"].Value;
                    }
                    else
                    {
                        nomeFuncionario = Regex.Match(trecho, @"(?i)\d\s(\/\s)?\d\s(?<nomeFuncionario>\w.*)\n\/\nFun..o");

                        if (nomeFuncionario.Groups["nomeFuncionario"].Success &&
                        nomeFuncionario.Success &&
                        !string.IsNullOrEmpty(nomeFuncionario.Value))
                        {
                            colaborador.NomeFuncionario = nomeFuncionario.Groups["nomeFuncionario"].Value;
                        }
                    }

                    if (funcaoFuncionario.Groups["funcaoColaborador"].Success &&
                        funcaoFuncionario.Success &&
                        !string.IsNullOrEmpty(funcaoFuncionario.Value))
                    {
                        colaborador.FuncaoFuncionario = funcaoFuncionario.Groups["funcaoColaborador"].Value;
                    }

                    if (mesAno.Groups["mesAno"].Success &&
                        mesAno.Success &&
                        !string.IsNullOrEmpty(mesAno.Value))
                    {
                        colaborador.MesAno = Convert.ToDateTime(mesAno.Groups["mesAno"].Value.Replace("MARCO", "MARÇO"));
                    }
                    else
                    {
                        mesAno = Regex.Match(trecho, @"(?i)\n(?<mesAno>\w.*\/\d{4})\nPAGAMENTO");
                        var mesAno2 = Regex.Match(trecho, @"(?i)\n(?<mesAno>\w.*\/\d{4})\nCNPJ");

                        bool isMesAno1 = mesAno.Success && !string.IsNullOrEmpty(mesAno.Value);
                        bool isMesAno2 = mesAno2.Success && !string.IsNullOrEmpty(mesAno2.Value);

                        if (isMesAno1)
                            colaborador.MesAno = Convert.ToDateTime(mesAno.Groups["mesAno"].Value.Replace("MARCO", "MARÇO"));
                        else if (isMesAno2)
                            colaborador.MesAno = Convert.ToDateTime(mesAno2.Groups["mesAno"].Value.Replace("MARCO", "MARÇO"));
                    }

                    if (salContrINSS.Groups["salContrINSS"].Success &&
                        salContrINSS.Success &&
                        !string.IsNullOrEmpty(salContrINSS.Value))
                    {
                        var valorSalContrINSS = salContrINSS.Groups["salContrINSS"].Value.Trim().Split("R$");

                        if (valorSalContrINSS.Length == 2)
                        {
                            salContrINSS = Regex.Match(trecho, @"(?i)IRF\n\s(?<salContrINSS>\w.*\n\s\w.*)");
                            valorSalContrINSS = salContrINSS.Groups["salContrINSS"].Value.Trim().Split("R$");
                        }

                        colaborador.SalContrINSS = Convert.ToDecimal(valorSalContrINSS[2].Trim());
                    }

                    if (!colaborador.IsValid())
                        throw new Exception($"Não foi possível obter dados do funcionário\ndados:{trecho}");

                    if (!_colaboradores.Any(x => x.NomeFuncionario.ToLower() == colaborador.NomeFuncionario.ToLower() && x.MesAno == colaborador.MesAno))
                        _colaboradores.Add(colaborador);
                }

            }
            catch (Exception ex)
            {

            }
        }

        private List<Domain.ExtratorDadosSalContINSS.Entities.Colaborador> InserirDadosDOsColaboradores()
        {
            var colaboradorAgrupados = _colaboradores.GroupBy(x => x.NomeFuncionario);
            var colaboradores = new List<Domain.ExtratorDadosSalContINSS.Entities.Colaborador>();

            foreach (var colaboradorAgrupado in colaboradorAgrupados)
            {
                var colaborador = new Domain.ExtratorDadosSalContINSS.Entities.Colaborador();

                foreach (var _colaborador in colaboradorAgrupado.Select(s => s))
                {
                    if (string.IsNullOrEmpty(colaborador.NomeCompleto))
                        colaborador.NomeCompleto = _colaborador.NomeFuncionario;

                    colaborador.ReciboPagamentos.Add(new Domain.ExtratorDadosSalContINSS.Entities.ReciboPagamento
                    {
                        INSS_PROC = _colaborador.SalContrINSS,
                        Data = _colaborador.MesAno
                    });
                }

                colaborador.GerarDecimoTerceiro();
                colaboradores.Add(colaborador);
            }

            return colaboradores;
        }

        public void ExportarArquivoBase(string path, string pathDestino)
        {
            try
            {
                var files = LerArquivos(path);
                ExtrairDadosColaboradores(files);
                var colaboradores = InserirDadosDOsColaboradores();
                new CrossCutting.Write.Excel.ExtratorDadosSalContINSS.ManipularExcel()
                    .ExportarExcel(colaboradores, pathDestino);
            }
            catch (Exception)
            {

            }
        }

        public void InserirDadosNoExcel(string path, IEnumerable<Domain.ExtratorDadosSalContINSS.Entities.Colaborador> colaboradores)
        {
            new CrossCutting.Write.Excel.ExtratorDadosSalContINSS.ManipularExcel().PreencherDadosNoExcel(path, colaboradores);
        }
        
    }
}
