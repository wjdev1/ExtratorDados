using ExtratorDados.CrossCutting.Read.PDF.Rais;
using ExtratorDados.Domain.Rais.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExtratorDados.Application.RAIS
{
    public class RAISService
    {
        private readonly RAISLeituraPDFService _service;
        private readonly string _destino;

        public RAISService(string destino = null)
        {
            _service = new RAISLeituraPDFService();
            _destino = destino ?? $"c:\\temp\\FGTS_NAO_PAGOS\\"; 
        }

        public async Task<IEnumerable<ArquivoCompletoRAIS>> ArquivoCompletoRAIs()
        {
            return await _service.ObterDadosColaboradoresRAIS();
        }

        public async Task GerarArquivoCSV(string destino = null)
        {
            try
            {
                var colaboradores = await _service.ObterDadosColaboradoresRAIS();
                bool ehNovoCSV;
                string nomeCSV = string.Empty;
                string dtAdmissao = string.Empty;
                string filename = string.Empty;
                var lines = new List<string>();

                //colaboradores = colaboradores.Where(c => c.Nome.ToUpper().StartsWith("FERNANDO DOS SANTOS DIAS") && c.CPF.StartsWith("016"));

                colaboradores.ToList().ForEach(colaborador =>
                {
                    //if (colaborador.Ano == 2014 && colaborador.MesAno == "2/2014")
                    //{

                    //}

                    ehNovoCSV = nomeCSV != $"{colaborador.Nome.ToUpper()}_{colaborador.CPF.Substring(0, 3)}";
                    bool novaDataAdmissao = dtAdmissao != colaborador.DataAdmissao;

                    if (lines.Count > 1 && (ehNovoCSV || novaDataAdmissao))
                    {
                        using (var file = File.CreateText(filename))
                        {
                            foreach (var arr in lines)
                            {
                                file.WriteLine(string.Join(",", arr).Replace("'", ""));
                            }
                        }

                        lines = new List<string>
                    {
                        "MES_ANO;VALOR;FGTS;FGTS_REC.;CONTRIBUICAO_SOCIAL;CONTRIBUICAO_SOCIAL_REC."
                    };
                    }

                    bool ehMesValido = colaborador.MesAno != "13º adiantamento" &&
                                           colaborador.MesAno != "13º parcela final" &&
                                           colaborador.MesAno != "Aviso prévio";

                    if (ehMesValido)
                    {
                        #region REGRA 1.2
                        bool ehCompetenciaSemOcorrencias = colaborador.CompNaoLocalizada == "Sem Ocorrências";
                        #endregion

                        #region REGRA 1.3
                        bool ehMesDepositado = colaborador.MesesNaoDepositados == "1";

                        var qtdDepositosNoAno = colaboradores.Count(col => col.Ano == colaborador.Ano &&
                                                                            col.CPF == colaborador.CPF &&
                                                                            col.DataAdmissao == colaborador.DataAdmissao &&
                                                                            col.CPF == colaborador.CPF &&
                                                                            col.MesesNaoDepositados == "1" &&
                                                                            col.MesAno != "13º adiantamento" &&
                                                                            col.MesAno != "13º parcela final" &&
                                                                            col.MesAno != "Aviso prévio");

                        var qtdMesesColaboradorNoAno = colaboradores.Count(col => col.Ano == colaborador.Ano &&
                                                                       col.CPF == colaborador.CPF &&
                                                                       col.DataAdmissao == colaborador.DataAdmissao &&
                                                                       col.CPF == colaborador.CPF &&
                                                                       col.MesAno != "13º adiantamento" &&
                                                                            col.MesAno != "13º parcela final" &&
                                                                            col.MesAno != "Aviso prévio");

                        var ultimoMesDepositado = colaboradores.LastOrDefault(col => col.Ano == colaborador.Ano &&
                                                                            col.CPF == colaborador.CPF &&
                                                                            col.DataAdmissao == colaborador.DataAdmissao &&
                                                                            col.CPF == colaborador.CPF &&
                                                                            col.MesAno != "13º adiantamento" &&
                                                                            col.MesAno != "13º parcela final" &&
                                                                            col.MesAno != "Aviso prévio");

                        bool faltouUmMes = qtdDepositosNoAno < qtdMesesColaboradorNoAno &&
                                                   qtdMesesColaboradorNoAno - qtdDepositosNoAno == 1;

                        bool podeDesconsiderarUltimoMes = faltouUmMes && ultimoMesDepositado.MesesNaoDepositados.Trim() == "";

                        #endregion

                        #region REGRA 2.2
                        bool ehColaboradorCompNaoLocalizada = colaborador.CompNaoLocalizada == "1";
                        bool ehColaboradorMesDepositado = colaborador.CompNaoLocalizada.Trim() == "";
                        bool ehDepositoEfetuado = colaborador.MesesNaoDepositados == "1";
                        bool ehPagamentoNaoIdentificado = colaborador.CompNaoLocalizada.Trim() == "" &&
                                                          colaborador.MesesNaoDepositados.Trim() == "";

                        #endregion

                        bool ehRegistroCSV = !ehCompetenciaSemOcorrencias &&
                            !ehMesDepositado &&
                            !podeDesconsiderarUltimoMes &&
                            ehColaboradorCompNaoLocalizada &&
                            !ehColaboradorMesDepositado &&
                            !ehDepositoEfetuado;

                        if (ehRegistroCSV || (ehPagamentoNaoIdentificado && !podeDesconsiderarUltimoMes))
                        {
                            #region REGRA 2.3
                            bool ehAnoTerminaEmDezembro = DateTime.Parse(ultimoMesDepositado.MesAno).Month == 12;

                            if (ehAnoTerminaEmDezembro && DateTime.Parse(colaborador.MesAno).Month == 11)
                            {
                                colaborador.Remuneracao += colaboradores.FirstOrDefault(col => col.Ano == colaborador.Ano &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.DataAdmissao == colaborador.DataAdmissao &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.MesAno == "13º adiantamento")?.Remuneracao ?? 0;
                            }
                            else if (ehAnoTerminaEmDezembro && DateTime.Parse(colaborador.MesAno).Month == 12)
                            {
                                colaborador.Remuneracao += colaboradores.FirstOrDefault(col => col.Ano == colaborador.Ano &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.DataAdmissao == colaborador.DataAdmissao &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.MesAno == "13º parcela final")?.Remuneracao ?? 0;

                                colaborador.Remuneracao += colaboradores.FirstOrDefault(col => col.Ano == colaborador.Ano &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.DataAdmissao == colaborador.DataAdmissao &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.MesAno == "Aviso prévio")?.Remuneracao ?? 0;
                            }

                            if (!ehAnoTerminaEmDezembro)
                            {
                                colaborador.Remuneracao += colaboradores.FirstOrDefault(col => col.Ano == colaborador.Ano &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.DataAdmissao == colaborador.DataAdmissao &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.MesAno == "13º adiantamento")?.Remuneracao ?? 0;

                                colaborador.Remuneracao += colaboradores.FirstOrDefault(col => col.Ano == colaborador.Ano &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.DataAdmissao == colaborador.DataAdmissao &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.MesAno == "13º parcela final")?.Remuneracao ?? 0;

                                colaborador.Remuneracao += colaboradores.FirstOrDefault(col => col.Ano == colaborador.Ano &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.DataAdmissao == colaborador.DataAdmissao &&
                                                                                col.CPF == colaborador.CPF &&
                                                                                col.MesAno == "Aviso prévio")?.Remuneracao ?? 0;
                            }

                            #endregion

                            if (ehNovoCSV || novaDataAdmissao)
                            {
                                nomeCSV = $"{colaborador.Nome.ToUpper()}_{colaborador.CPF.Substring(0, 3)}";
                                dtAdmissao = colaborador.DataAdmissao.ToString();
                                filename = $"{_destino}{nomeCSV}.csv";
                                int count = 2;

                                while (File.Exists($"{filename}"))
                                {
                                    filename = $"{_destino}{nomeCSV}_{count}.csv";
                                    count++;
                                }

                                lines = new List<string>
                            {
                               "MES_ANO;VALOR;FGTS;FGTS_REC.;CONTRIBUICAO_SOCIAL;CONTRIBUICAO_SOCIAL_REC."
                            };
                            }

                            lines.Add($" {colaborador.MesAno.PadLeft(7, '0')};{colaborador.Remuneracao};S;N;N;N");
                        }
                    }
                });

                if (lines.Count > 1)
                {
                    using (var file = File.CreateText(filename))
                    {
                        foreach (var arr in lines)
                        {
                            file.WriteLine(string.Join(",", arr).Replace("'", ""));
                        }
                    }

                    lines = new List<string>();
                }
            }
            catch (Exception ex)
            {

            }
        }


    }
}
