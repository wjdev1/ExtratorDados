using ExtratorDados.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static ExtratorDados.Domain.Services.BaseService;

namespace ExtratorDados.CrossCutting.Read.TXT.Services
{
    public class ReadTXTService
    {
        public event OnNotificador _onNotificar;
        private readonly string _arquivoTXT = Environment.CurrentDirectory + "\\result.tmp";
        private readonly List<string> _listaParaPularLinha = new List<string>
        {
            "Fls.:",
            "RELAÇÃO CALCULO",
            "C.Custo:",
            "Cod. Tp Descrição Referência Valor Cod. Tp Descrição Referência Valor",
            "Assinado eletronicamente por:",
            "https://pje.trt12.jus.br/primeirograu/Processo/ConsultaDocumento/listView.seam?nd",
            "Número do processo:",
            "Número do documento:"
        };
        private List<Colaborador> _colaboradores = new List<Colaborador>();

        public List<Colaborador> LerTXT()
        {
            if (!File.Exists(_arquivoTXT))
                throw new Exception("Arquivo temporário não foi encontrado");

            int qtdLinha = 0;

            _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.efetuando_leitura_de_arquivo__0_, "temp"), null, false);

            try
            {

                Colaborador colaborador = null;
                int codigoColaborador = 0;
                decimal salario = 0;
                string periodo = string.Empty;
                bool isCalculoMensal = false;
                int totalLinhas = 0;

                using (var stTotal = new StreamReader(_arquivoTXT))
                {
                    totalLinhas = stTotal.ReadToEnd().Split(new char[] { '\n' }).Count();
                };

                using (StreamReader sr = new StreamReader(_arquivoTXT))
                {

                    while (!sr.EndOfStream)
                    {
                        var linha = sr.ReadLine();
                        if (linha != null)
                        {
                            qtdLinha++;

                            var v = Math.Round(qtdLinha / Convert.ToDecimal(totalLinhas), 4);
                            _onNotificar?.Invoke(string.Format(Domain.Resources.Notificacao.efetuando_leitura_de_arquivo__0_, "temp"), Math.Round(qtdLinha / Convert.ToDecimal(totalLinhas), 3), true);

                            if (_listaParaPularLinha.Any(texto => linha.StartsWith(texto)))
                            {
                                continue;
                            }

                            var rgxPeriodoEtipoFolha = Regex.Match(linha, @"(?i)(?<data>\d{2}\/\d{2}\/\d{4}) a \d{2}\/\d{2}\/\d{4} Tipo:\s?(?<tipoFolha>\w.*)");
                            var rgxINSS_PROC = Regex.Match(linha, @"(?i)INSS Proc.\s*(?<vlrINSSProc>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxSalarioBase = Regex.Match(linha, @"(?i)Sal.rio Base.\s*(?<salarioBase>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxDataDemissao = Regex.Match(linha, @"(?i)Demiss.o.(\s)?(\n)?(?<dataDemissao>\d{2}\/\d{2}\/\d{4})");

                            #region REGEX_VALORES_PARA_SEREM_OBTIDOS
                            //valores as serem obtidos
                            var rgxNomeColaborador = Regex.Match(linha, @"(?i)Colaborador: (?<codColaborador>\d+) \- (?<nomeColaborador>\w.*)(Admiss.o)");
                            var rgxCodColaborador = Regex.Match(linha, @"(?i)Colaborador: (?<codColaborador>\d+)");
                            var rgxCargoColaborador = Regex.Match(linha, @"(?i)Cargo:\s(?<cargo>\w.*)Sal.rio");

                            var rgxHSNormais_Cod01 = Regex.Match(linha, @"(?i)1 01 Horas Normais\s*(?<horasNormais>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxHSFeriasRef_Cod12 = Regex.Match(linha, @"(?i)12 01 Horas Férias Diurnas\s*(?<horasFeriasRef>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxHSAuxDoencaINSSRef_Cod28 = Regex.Match(linha, @"(?i)28 04 Horas Aux.Doen.a INSS\s*(?<hsAuxDoenca>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxHSAfastRef_Cod56 = Regex.Match(linha, @"(?i)56 01 Horas Salario Doen.a\s*(?<horasAfasRef>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxVlrInsalubridade_Cod62 = Regex.Match(linha, @"(?i)62.01.Insalubridade\s*(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?\s*(?<vlrInsalubridade>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");
                            var rgxVlrPericulosidade_Cod64 = Regex.Match(linha, @"(?i)64.01.Periculosidade\s*(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?\s*(?<vlrPericulosidade>(([1-9]\d{0,2}(\.\d{3})*)|(([1-9]\.\d*)?\d))(\,\d\d)?)");


                            if (rgxPeriodoEtipoFolha.Groups["data"].Value != null && rgxPeriodoEtipoFolha.Success)
                            {
                                if (rgxPeriodoEtipoFolha.Groups["tipoFolha"].Value != "Cálculo Mensal")
                                {
                                    //regra passada, se arquivo pdf não for Calculo Mensal, não precisa obter informações
                                    isCalculoMensal = false;
                                    continue;
                                }
                                else
                                    isCalculoMensal = true;

                                periodo = rgxPeriodoEtipoFolha.Groups["data"].Value;
                            }
                            else if (!string.IsNullOrEmpty(periodo) && isCalculoMensal)
                            {
                                if (rgxNomeColaborador.Groups["nomeColaborador"].Value != null && rgxNomeColaborador.Success)
                                {

                                    colaborador = new Colaborador();
                                    var nome = rgxNomeColaborador.Groups["nomeColaborador"].Value.TrimEnd().Replace("Licença de", "");
                                    var _nome = Regex.Replace(nome, @"\d", "").TrimEnd();
                                    colaborador.Nome = _nome.TrimEnd();
                                    colaborador.SetMesAno(DateTime.Parse(periodo));
                                    colaborador.Data = DateTime.Parse(periodo);
                                }

                                if (rgxCodColaborador.Groups["codColaborador"].Value != null && rgxCodColaborador.Success)
                                {
                                    int.TryParse(rgxCodColaborador.Groups["codColaborador"].Value, out codigoColaborador);
                                    colaborador.Codigo = codigoColaborador;

                                    bool jaExistePeriodoInformado = _colaboradores.Any(x => periodo == "01/01/2011" && x.Data.Date == new DateTime(2011, 1, 1).Date && x.Codigo == codigoColaborador);

                                    if (jaExistePeriodoInformado)
                                    {
                                        var qtd = _colaboradores.Count(x => periodo == "01/01/2011" && x.Data.Date == new DateTime(2011, 1, 1).Date && colaborador.Codigo == codigoColaborador);
                                        var novaData = new DateTime(2011, 2, 1);
                                        colaborador.SetMesAno(novaData);
                                        colaborador.Data = novaData;

                                        if (colaborador.Codigo == 16918)
                                        {

                                        }
                                    }
                                }

                                if (rgxCargoColaborador.Groups["cargo"].Value != null && rgxCargoColaborador.Success)
                                {
                                    var cargo = rgxCargoColaborador.Groups["cargo"].Value;
                                    colaborador.Cargo = cargo.TrimEnd();
                                }

                                if (rgxSalarioBase.Groups["salarioBase"].Value != null && rgxSalarioBase.Success)
                                {
                                    decimal.TryParse(rgxSalarioBase.Groups["salarioBase"].Value, out salario);
                                    colaborador.SalarioBase = salario;
                                }

                                if (rgxDataDemissao.Groups["dataDemissao"].Value != null && rgxDataDemissao.Success)
                                {
                                    colaborador.Demissao = rgxDataDemissao.Groups["dataDemissao"].Value;
                                }

                                if (rgxHSNormais_Cod01.Groups["horasNormais"].Value != null && rgxHSNormais_Cod01.Success)
                                {
                                    colaborador.HS_Normais = decimal.Parse(rgxHSNormais_Cod01.Groups["horasNormais"].Value);
                                }

                                if (rgxHSFeriasRef_Cod12.Groups["horasFeriasRef"].Value != null && rgxHSFeriasRef_Cod12.Success)
                                {
                                    colaborador.HS_Ferias_Referencia = decimal.Parse(rgxHSFeriasRef_Cod12.Groups["horasFeriasRef"].Value);
                                }

                                if (rgxHSAuxDoencaINSSRef_Cod28.Groups["hsAuxDoenca"].Value != null && rgxHSAuxDoencaINSSRef_Cod28.Success)
                                {
                                    colaborador.HS_Aux_Doenca = decimal.Parse(rgxHSAuxDoencaINSSRef_Cod28.Groups["hsAuxDoenca"].Value);
                                }

                                if (rgxHSAfastRef_Cod56.Groups["horasAfasRef"].Value != null && rgxHSAfastRef_Cod56.Success)
                                {
                                    colaborador.HS_Afas_Refer = decimal.Parse(rgxHSAfastRef_Cod56.Groups["horasAfasRef"].Value);
                                }

                                if (rgxVlrInsalubridade_Cod62.Groups["vlrInsalubridade"].Value != null && rgxVlrInsalubridade_Cod62.Success)
                                {
                                    colaborador.InsalubridadeValor = decimal.Parse(rgxVlrInsalubridade_Cod62.Groups["vlrInsalubridade"].Value);
                                }

                                if (rgxVlrPericulosidade_Cod64.Groups["vlrPericulosidade"].Value != null && rgxVlrPericulosidade_Cod64.Success)
                                {
                                    colaborador.PericulosidadeValor = decimal.Parse(rgxVlrPericulosidade_Cod64.Groups["vlrPericulosidade"].Value);
                                }

                                if (rgxINSS_PROC.Groups["vlrINSSProc"].Value != null && rgxINSS_PROC.Success)
                                {
                                    colaborador.INSS_PROC = decimal.Parse(rgxINSS_PROC.Groups["vlrINSSProc"].Value);
                                    _colaboradores.Add(colaborador);
                                    colaborador = null;
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string Result = $"log_erro_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.txt";
                using (var writer = new StreamWriter(Result))
                    writer.Write($"ERRO: Na leitura do arquivo txt gerado\n{ex.Message}\nLinha:{qtdLinha}");

                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }

            if (File.Exists(_arquivoTXT))
                File.Delete(_arquivoTXT);

            _onNotificar.Invoke(string.Format(Domain.Resources.Notificacao.excluindo_arquivo__0_, "temp"), null, false);

            return _colaboradores;
        }
    }
}
