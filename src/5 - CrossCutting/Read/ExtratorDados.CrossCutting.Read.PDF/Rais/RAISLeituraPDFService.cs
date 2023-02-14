using ExtratorDados.CrossCutting.Read.PDF.Services;
using ExtratorDados.Domain.Rais.Entities;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtratorDados.CrossCutting.Read.PDF.Rais
{
    public class RAISLeituraPDFService : BaseReadPDFService
    {
        public List<Colaborador> Colaboradores = new List<Colaborador>();
        public List<CompNaoLocalizada> CompetenciasNaoLocalizadas = new List<CompNaoLocalizada>();
        public List<ExtratoFGTS> ExtratosFGTS = new List<ExtratoFGTS>();

        private readonly Dados.RAIS.RAISRepository<Colaborador> _repositoryColaborador = new Dados.RAIS.RAISRepository<Colaborador>();
        public List<DecimoTerceiroEAviso> DecimoTerceiroEAvisos = new List<DecimoTerceiroEAviso>();

        #region Fields COMP NÃO LOCALIZADA
        bool _varrerCompNaoLocalizadas = false;
        bool _varrerExtratoPago = false;
        bool _buscarPIS = false;
        Colaborador ColaboradorCompNaoLocalizada;
        #endregion

        #region Fields ExtratoFGTS
        bool _varrerColaboradorExtratoFGTS = false;
        int _ultimaPaginaExtratoFGTS = 0;
        int _linhasPercorridasExtratoFGTS = 0;
        Colaborador _colaboradorExtratoFGTS;
        bool _proxPaginaExtratoFGTS = false;
        bool _varrerSaldoExtratoFGTS = false;
        #endregion

        public void ManipularDadosRAIS(StringBuilder builder)
        {
            var ano = 0;
            var nome = string.Empty;
            var pis = string.Empty;
            var cpf = string.Empty;
            var dataAdmissao = DateTime.MinValue;
            DateTime? dataDeslig = null;
            int? codDeslig = null;

            var anoRgx = System.Text.RegularExpressions.Regex.Match(builder.ToString(), "(?i)ESTABELECIMENTO\\s(?<anoRais>\\d{4})");
            if (anoRgx.Success) ano = int.Parse(anoRgx.Groups["anoRais"].Value);

            var trecho = builder.ToString().Split("VÍNCULO");

            for (int texto = 1; texto < trecho.Length; texto++)
            {
                var nomeRgx = System.Text.RegularExpressions.Regex.Match(trecho[texto].Trim(), @"(?i)PIS.\w*.*NOME.(?<nome>(\s?)\w.*.)");
                var pisRgx = System.Text.RegularExpressions.Regex.Match(trecho[texto], "(?i)PIS.(\\s)?(?<pis>\\w.*[0-9])");
                var cpfRgx = System.Text.RegularExpressions.Regex.Match(trecho[texto], "(?i)CPF.(\\s)?(?<cpf>\\d{3}\\.\\d{3}\\.\\d{3}\\-\\d{2})");
                var dataAdmissaoRgx = System.Text.RegularExpressions.Regex.Match(trecho[texto], @"(?<dtAdmissao>\d{2}\/\d{2}\/\d{4})(\s)w?\w.*(\s)?\n(?i)Data de Admiss.o");
                var desligamentoRgx = System.Text.RegularExpressions.Regex.Match(trecho[texto], @"(?<dataDeslig>\d{2}\/\d{2})\nData.\n\w.*\nCausa.(\s)?(?<codDeslig>\d*)");
                var salarioRgx = System.Text.RegularExpressions.Regex.Match(trecho[texto], @"(?<dataDeslig>\d{2}\/\d{2})\nData.\n\w.*\nCausa.(\s)?(?<codDeslig>\d*)");
                if (nomeRgx.Success) nome = nomeRgx.Groups["nome"].Value.Trim();
                if (pisRgx.Success) pis = pisRgx.Groups["pis"].Value.Trim();
                if (cpfRgx.Success) cpf = cpfRgx.Groups["cpf"].Value.Trim();
                if (dataAdmissaoRgx.Success) dataAdmissao = DateTime.Parse(dataAdmissaoRgx.Groups["dtAdmissao"].Value);
                if (desligamentoRgx.Success) dataDeslig = desligamentoRgx.Groups["dataDeslig"].Value != null ? DateTime.Parse(desligamentoRgx.Groups["dataDeslig"].Value + "/" + ano) : null;
                if (desligamentoRgx.Success) codDeslig = desligamentoRgx.Groups["codDeslig"].Value != null ? int.Parse(desligamentoRgx.Groups["codDeslig"].Value) : null;

                Colaboradores.Add(new Colaborador(ano, nome, pis, cpf, dataAdmissao, dataDeslig, codDeslig));
            }
        }
        public void ManipularDadosRAISRemuneracao(string texto)
        {
            var ano = 0;
            string pis = string.Empty;
            string nome = string.Empty;
            var anoRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?i)Ano Base.(\s?)(?<ano>\d{4})");
            if (anoRgx.Success) ano = int.Parse(anoRgx.Groups["ano"].Value);

            var textoSplit = texto.Split("\n");

            for (int linha = 0; linha < textoSplit.Length; linha++)
            {
                var pis_e_nomeRgx = System.Text.RegularExpressions.Regex.Match(textoSplit[linha], @"(?i)PIS\:");

                if (pis_e_nomeRgx.Success)
                {
                    linha += 3;
                    var trechoSplit = textoSplit[linha].Split(" ");
                    pis = trechoSplit[0];
                    nome = textoSplit[linha].Replace(trechoSplit[0], "").Trim();
                }

                var colaborador = Colaboradores
                    .Where(col => col.PIS == pis && col.Ano == ano)
                    .FirstOrDefault();

                if (colaborador != null)
                {
                    var isTrechoRemuneracao = textoSplit[linha].Contains(@"Admissão de empregado");

                    if (isTrechoRemuneracao)
                    {
                        linha++;
                        for (int mes = 1; mes <= 12; mes++)
                        {
                            colaborador.Remuneracoes
                                .Add(new Remuneracao
                                {
                                    CPFColaborador = colaborador.CPF,
                                    ValorPagamento = decimal.Parse(textoSplit[linha]),
                                    DataPagamento = new DateTime(ano, mes, 1)
                                });

                            linha++;
                        }

                        colaborador = null;
                    }
                }
            }
        }

        public void ManipularDadosRAISDecimoTerceiroEAviso(string texto)
        {
            decimal avisoPrevio = 0;
            decimal decTerceiroAdiantamento = 0;
            decimal decTerceiroParcFinal = 0;
            int ano = 0;
            string pis = string.Empty;
            Colaborador _colaborador = null;

            var anoRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?i)ESTABELECIMENTO\s(?<anoRais>\d{4})");
            if (anoRgx.Success) ano = int.Parse(anoRgx.Groups["anoRais"].Value);

            var splitPorVinculo = texto.Split("VÍNCULO");

            if (splitPorVinculo.Length == 3)
            {
                for (int vinculo = 1; vinculo <= 2; vinculo++)
                {
                    var dataAdmissao = DateTime.MinValue;
                    DateTime? dataDeslig = null;
                    int? codDeslig = null;

                    var pisRgx = System.Text.RegularExpressions.Regex.Match(splitPorVinculo[vinculo], @"(?i)PIS.(\s?)(?<pis>\w*.*)Nome");
                    var avisoPrevioRgx = System.Text.RegularExpressions.Regex.Match(splitPorVinculo[vinculo], @"(?i)Aviso Pr.vio.(\s?)(?<avisoPrevio>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                    var decTerceiroAdiantamentoRgx = System.Text.RegularExpressions.Regex.Match(splitPorVinculo[vinculo], @"(?i)13. Adiantamento\w*.*");
                    var dataAdmissaoRgx = System.Text.RegularExpressions.Regex.Match(splitPorVinculo[vinculo], @"(?<dtAdmissao>\d{2}\/\d{2}\/\d{4})(\s)w?\w.*(\s)?\n(?i)Data de Admiss.o");
                    var desligamentoRgx = System.Text.RegularExpressions.Regex.Match(splitPorVinculo[vinculo], @"(?<dataDeslig>\d{2}\/\d{2})\nData.\n\w.*\nCausa.(\s)?(?<codDeslig>\d*)");

                    if (pisRgx.Success)
                    {
                        _colaborador = new Dados.RAIS.RAISRepository<Colaborador>().ObterColaboradorPorPIS(pisRgx.Groups["pis"].Value.Replace(".",""));

                        if (_colaborador.CPF == "827.700.349-87")
                        {

                        }

                        if (_colaborador == null) throw new Exception("Colaborador inválido!");
                    }

                    if (avisoPrevioRgx.Success) avisoPrevio = decimal.Parse(avisoPrevioRgx.Groups["avisoPrevio"].Value);
                    if (dataAdmissaoRgx.Success) dataAdmissao = DateTime.Parse(dataAdmissaoRgx.Groups["dtAdmissao"].Value);
                    if (desligamentoRgx.Success) dataDeslig = desligamentoRgx.Groups["dataDeslig"].Value != null ? DateTime.Parse(desligamentoRgx.Groups["dataDeslig"].Value + "/" + ano) : null;
                    if (desligamentoRgx.Success) codDeslig = desligamentoRgx.Groups["codDeslig"].Value != null ? int.Parse(desligamentoRgx.Groups["codDeslig"].Value) : null;

                    if (decTerceiroAdiantamentoRgx.Success)
                    {
                        var trechoSplit = decTerceiroAdiantamentoRgx.Value
                            .Split(" ")
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToArray();

                        decTerceiroAdiantamento = decimal.Parse(trechoSplit[3]);

                        if(trechoSplit.Length == 12)
                            decTerceiroParcFinal = decimal.Parse(trechoSplit[9]);
                        else
                            decTerceiroParcFinal = decimal.Parse(trechoSplit[8]);

                        var decTerceiro = new DecimoTerceiroEAviso
                        {
                            CPF = _colaborador.CPF,
                            Ano = ano,
                            DataAdmissao = dataAdmissao,
                            DataDesligamento = dataDeslig,
                            CodCausaDesligamento = codDeslig,
                            AvisoPrevio = avisoPrevio,
                            DecTerceiroAdiantamento = decTerceiroAdiantamento,
                            DecTerceiroParcFinal = decTerceiroParcFinal
                        };

                        new Dados.RAIS.RAISRepository<DecimoTerceiroEAviso>().Add(decTerceiro);                        
                    }
                }
            }
            else
            {
                var dataAdmissao = DateTime.MinValue;
                DateTime? dataDeslig = null;
                int? codDeslig = null;

                var pisRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?i)PIS.(\s?)(?<pis>\w*.*)Nome");
                var avisoPrevioRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?i)Aviso Pr.vio.(\s?)(?<avisoPrevio>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                var decTerceiroAdiantamentoRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?i)13. Adiantamento\w*.*");
                var dataAdmissaoRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?<dtAdmissao>\d{2}\/\d{2}\/\d{4})(\s)w?\w.*(\s)?\n(?i)Data de Admiss.o");
                var desligamentoRgx = System.Text.RegularExpressions.Regex.Match(texto, @"(?<dataDeslig>\d{2}\/\d{2})\nData.\n\w.*\nCausa.(\s)?(?<codDeslig>\d*)");

                if (pisRgx.Success)
                {
                    _colaborador = new Dados.RAIS.RAISRepository<Colaborador>().ObterColaboradorPorPIS(pisRgx.Groups["pis"].Value.Replace(".", ""));

                    if (_colaborador.CPF == "827.700.349-87")
                    {

                    }

                    if (_colaborador == null) throw new Exception("Colaborador inválido!");
                }

                if (avisoPrevioRgx.Success) avisoPrevio = decimal.Parse(avisoPrevioRgx.Groups["avisoPrevio"].Value);
                if (dataAdmissaoRgx.Success) dataAdmissao = DateTime.Parse(dataAdmissaoRgx.Groups["dtAdmissao"].Value);
                if (desligamentoRgx.Success) dataDeslig = desligamentoRgx.Groups["dataDeslig"].Value != null ? DateTime.Parse(desligamentoRgx.Groups["dataDeslig"].Value + "/" + ano) : null;
                if (desligamentoRgx.Success) codDeslig = desligamentoRgx.Groups["codDeslig"].Value != null ? int.Parse(desligamentoRgx.Groups["codDeslig"].Value) : null;

                if (decTerceiroAdiantamentoRgx.Success)
                {
                    var trechoSplit = decTerceiroAdiantamentoRgx.Value
                        .Split(" ")
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();

                    decTerceiroAdiantamento = decimal.Parse(trechoSplit[3]);

                    if (trechoSplit.Length == 12)
                        decTerceiroParcFinal = decimal.Parse(trechoSplit[9]);
                    else
                        decTerceiroParcFinal = decimal.Parse(trechoSplit[8]);

                    var decTerceiro = new DecimoTerceiroEAviso
                    {
                        CPF = _colaborador.CPF,
                        Ano = ano,
                        DataAdmissao = dataAdmissao,
                        DataDesligamento = dataDeslig,
                        CodCausaDesligamento = codDeslig,
                        AvisoPrevio = avisoPrevio,
                        DecTerceiroAdiantamento = decTerceiroAdiantamento,
                        DecTerceiroParcFinal = decTerceiroParcFinal
                    };

                    new Dados.RAIS.RAISRepository<DecimoTerceiroEAviso>().Add(decTerceiro);
                }
            }
            
        }

        public void ManipularCompNaoLocalizadas(StringBuilder builder)
        {
            string nome = string.Empty;

            var texto = builder.ToString();

            var textoSplitadoPorLinha = texto.Split('\n').Select(s => s).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            //Trecho Extrato FGTS
            for (int linha = 0; linha < textoSplitadoPorLinha.Count(); linha++)
            {
                var caixaEconomixaRgx = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha], "(?i)C(\\s?)A(\\s?)I(\\s?)X(\\s?)A(\\s?)(\\s?).E(\\s?)C(\\s?).(\\s?)N(\\s?).(\\s?)M(\\s?)I(\\s?)C(\\s?)A(\\s?)(\\s?).F(\\s?)E(\\s?)D(\\s?)E(\\s?)R(\\s?)A(\\s?)L");
                var pisRgx = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha].Trim(), @"PIS\/PASEP");
                var competNaoLocalRgx = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha], @"(?i)COMPETENCIAS NAO LOCALIZADAS NESTA CONTA VINCULADA");
                var movContaPeriodoRgx = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha], @"(?i)MOVIMENTACAO DA CONTA NO PERIODO");

                var proxPagina = textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("Página");
                var linhaIdentificaFolha = textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("Fls.:");
                var linhaExtratoFilial = textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("ExtratoFGTSfilial");
                var linhaExtratoMatriz = textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("ExtratoFGTSmatriz");

                //Identificado como trecho para finalizar página
                if (proxPagina) return;

                if (linhaIdentificaFolha || linhaExtratoFilial || linhaExtratoMatriz) continue;

                if (caixaEconomixaRgx.Success)
                {
                    ColaboradorCompNaoLocalizada = null;
                    _varrerCompNaoLocalizadas = _varrerExtratoPago = false;
                }

                if (pisRgx.Success || _buscarPIS)
                {
                    if (pisRgx.Success) linha++;

                    if (textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("Página"))
                    {
                        _buscarPIS = true;
                        return;
                    }

                    _buscarPIS = false;

                    var pis = textoSplitadoPorLinha[linha].Trim().Split(' ')[0].Replace(".", "").Replace("-", "");

                    var colaborador = _repositoryColaborador.ObterColaboradorPorPIS(pis);

                    if (colaborador == null) throw new Exception("Colaborador não foi encontrado por PIS");

                    ColaboradorCompNaoLocalizada = colaborador;
                }

                //Validou trecho de competência não localizada
                if (competNaoLocalRgx.Success) _varrerCompNaoLocalizadas = true;

                if (movContaPeriodoRgx.Success) _varrerExtratoPago = true;

                #region TRECHO COMP. NÃO LOCALIZADAS
                if (_varrerCompNaoLocalizadas && !_varrerExtratoPago)//efetuando leitura no trecho do extrato
                {
                    linha++;

                    var datas = textoSplitadoPorLinha[linha].Trim().Split(' ').Where(d => d != "");
                    foreach (var data in datas)
                    {
                        var isData = DateTime.TryParse(data, out DateTime dt);

                        if (isData)
                        {
                            CompetenciasNaoLocalizadas.Add(new CompNaoLocalizada
                            {
                                PIS = ColaboradorCompNaoLocalizada.PIS,
                                MesAno = dt,
                            });
                        }
                        else if (textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("Página"))
                        {
                            _varrerCompNaoLocalizadas = true;
                            return;
                        }
                        else
                        {
                            _varrerCompNaoLocalizadas = false;
                        }
                    }
                }
                #endregion

                if (_varrerExtratoPago)
                {
                    _varrerCompNaoLocalizadas = false;
                    bool naoEhLinhaInicio = textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("MOVIMENTACAODACONTANOPERIODO");
                    bool trechoComPagamento = textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("DATASALDOANTERIOR");
                    var linhaSplit = textoSplitadoPorLinha[linha].Split(' ').Where(d => d != "").ToArray();

                    if (textoSplitadoPorLinha[linha].Trim().Replace(" ", "").StartsWith("Página"))
                    {
                        _varrerExtratoPago = true;
                        return;
                    }
                    else if (naoEhLinhaInicio || trechoComPagamento)
                    {

                    }
                    else
                    {
                        if(DateTime.TryParse(linhaSplit[0], out DateTime dtPago))
                        {
                            ExtratosFGTS.Add(new ExtratoFGTS 
                            { 
                                Data = dtPago, 
                                CPF = ColaboradorCompNaoLocalizada.CPF,
                                Valor = decimal.Parse(linhaSplit[linhaSplit.Length -1]) 
                            });
                        }
                        else
                        {
                            _varrerExtratoPago = false;
                        }
                    }
                }
            }
        }
        public void ManipularExtratoFGTS(StringBuilder builder, FileInfo file)
        {
            if (_ultimaPaginaExtratoFGTS != Int32.Parse(file.Name.Replace(".pdf", "")) -1)
            {
                _colaboradorExtratoFGTS = null;
                _varrerSaldoExtratoFGTS = false;
                _linhasPercorridasExtratoFGTS = 0;
            }

            _ultimaPaginaExtratoFGTS = Int32.Parse(file.Name.Replace(".pdf",""));
            _proxPaginaExtratoFGTS = false;

            var texto = builder.ToString();

            var textoSplitadoPorLinha = texto
                .Split('\n')
                .Select(s => s)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            for (int linha = 0; linha < textoSplitadoPorLinha.Length; linha++)
            {
                var trechoNovoColaborador = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha].Trim(), "################################################################");
                var saldoFGTSRgx = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha].Trim(), "(?i)SALDO ANTERIOR");
                var saldoTransportadoFGTSRgx = System.Text.RegularExpressions.Regex.Match(textoSplitadoPorLinha[linha].Trim(), "(?i)SALDO TRANSPORTADO");

                if (textoSplitadoPorLinha[linha].Trim().StartsWith("Fls.:") ||
                    textoSplitadoPorLinha[linha].Trim().StartsWith("Documento assinado"))
                {
                    continue;
                }

                if (trechoNovoColaborador.Success)
                {
                    _varrerColaboradorExtratoFGTS = true;
                    _colaboradorExtratoFGTS = null;
                    _varrerSaldoExtratoFGTS = false;
                    _linhasPercorridasExtratoFGTS = 0;
                }

                if (_varrerColaboradorExtratoFGTS)
                {
                    _linhasPercorridasExtratoFGTS++;
                }

                if (_linhasPercorridasExtratoFGTS == 4)
                {
                    var linhaSplit = textoSplitadoPorLinha[linha].Trim().Split(" ");
                    _colaboradorExtratoFGTS = _repositoryColaborador.ObterColaboradorPorPIS(linhaSplit[0]);

                    if (_colaboradorExtratoFGTS == null)
                    {

                    }

                    _linhasPercorridasExtratoFGTS = 0;
                    _varrerColaboradorExtratoFGTS = false;
                }

                if (textoSplitadoPorLinha[linha].Contains("ID.") && textoSplitadoPorLinha[linha].Contains("Pág."))
                {
                    _proxPaginaExtratoFGTS = true;  
                }

                if (saldoFGTSRgx.Success || saldoTransportadoFGTSRgx.Success)
                {
                    _varrerSaldoExtratoFGTS = true;
                }

                if (_varrerSaldoExtratoFGTS && 
                    _colaboradorExtratoFGTS != null &&
                    !_proxPaginaExtratoFGTS)
                {

                    if (_colaboradorExtratoFGTS.CPF == "056.246.729-77")
                    {

                    }
                    var trechoSplit = textoSplitadoPorLinha[linha]
                        .Trim()
                        .Split(" ")
                        .Select(s => s)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                    if (textoSplitadoPorLinha[linha].Replace(" ","").StartsWith("SALDOATRANSPORTAR"))
                    {
                        _colaboradorExtratoFGTS = null;
                        _varrerSaldoExtratoFGTS = false;
                        continue;
                    }

                    if (trechoSplit[0].StartsWith("SALDO"))
                    {
                        continue;
                    }

                    var data = trechoSplit[0];
                    var valor = trechoSplit.Last();
                    var descricao = textoSplitadoPorLinha[linha]
                        .Trim()
                        .Replace(data, "")
                        .Replace(valor, "");

                    if (DateTime.TryParse(data, out DateTime dt))
                    {
                        var extratoFGTS = new ExtratoFGTS
                        {
                            CPF = _colaboradorExtratoFGTS.CPF,
                            Descricao = descricao.Trim(),
                            Data = IncluiDataConformeDescricao(descricao.Trim(), dt),
                            Valor = decimal.Parse(valor)
                        };

                        new Dados.RAIS.RAISRepository<ExtratoFGTS>().Add(extratoFGTS);
                    }
                }
            }
        }

        public void MontarExcel()
        {
            var _repository = new Dados.RAIS.Repository();
            var dados = _repository.ObterDados();
        }

        private DateTime IncluiDataConformeDescricao(string descricao, DateTime dt)
        {
            if (descricao.ToUpper().StartsWith("DEPOSITO"))
            {
                var texto = descricao
                    .Replace("DEPOSITO 13-SALARIO","")
                    .Replace("DEPOSITO EM ATRASO", "")
                    .Replace("DEPOSITO RECURSAL", "")
                    .Replace("DEPOSITO","");

                var textoSplit = texto.Replace(" ", "").Split("/");
                
                if (textoSplit[0] == "JANEIR") textoSplit[0] = textoSplit[0].Replace("JANEIR", "JANEIRO");
                if (textoSplit[0] == "FEVEREIR") textoSplit[0] = textoSplit[0].Replace("FEVEREIR", "FEVEREIRO");
                if (textoSplit[0] == "MARC") textoSplit[0] = textoSplit[0].Replace("MARC", "MARÇO");
                if (textoSplit[0] == "OUTUBR") textoSplit[0] = textoSplit[0].Replace("OUTUBR", "OUTUBRO");
                if (textoSplit[0] == "NOVEMBR") textoSplit[0] = textoSplit[0].Replace("NOVEMBR", "NOVEMBRO");
                if (textoSplit[0] == "DEZEMBR") textoSplit[0] = textoSplit[0].Replace("DEZEMBR", "DEZEMBRO");
                if (textoSplit[0] == "MARCO") textoSplit[0] = textoSplit[0].Replace("C", "Ç");

                var mes = DateTime.ParseExact(textoSplit[0], "MMMM", CultureInfo.CurrentCulture).Month;

                if (textoSplit.Length == 1)
                    return dt;

                return new DateTime(int.Parse(textoSplit[1]), mes, 1);
            }


            return dt;
        }
    }
}
