using ExtratorDados.Domain.Entities;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using static ExtratorDados.Domain.Services.BaseService;

namespace ExtratorDados.CrossCutting.Write.CSV.Services
{
    public class WriteCSVService
    {
        private readonly List<Colaborador> _colaboradores;
        public event OnNotificador _onNotificar;
        private readonly bool _usarConfigPadrao;

        public WriteCSVService(List<Colaborador> colaboradores, bool usarConfigPadrao = false)
        {
            _colaboradores = colaboradores.OrderBy(o => o.Data).OrderBy(o => o.Nome).ToList();
            _usarConfigPadrao = usarConfigPadrao;
        }
        public void ExportarCSVLocalmente(string _destino = null)
        {
            _onNotificar.Invoke(string.Format(Domain.Resources.Notificacao.exportando_arquivo__0_, "csv"), null, false);

            foreach (var _codColaborador in _colaboradores.Select(s => s.Codigo).Distinct())
            {
                var nomeColaborador = _colaboradores.FirstOrDefault(x => x.Codigo == _codColaborador).Nome;

                ExportarSalarioBase(_codColaborador, nomeColaborador, _destino);
                ExportarINSS_Proc(_codColaborador, nomeColaborador, _destino);
                ExportarInsalubridade(_codColaborador, nomeColaborador, _destino);
                ExportarPericulosidade(_codColaborador, nomeColaborador, _destino);
            }
        }
        private void ExportarSalarioBase(int codColaborador, string nomeColaborador, string destino = null)
        {
            var salario_base_csv = _colaboradores.Where(x => x.Codigo == codColaborador).OrderBy(o => o.Data).Select(s => new { s.Nome, s.SalarioBase, s.Data });
            string path = ConfigurationManager.AppSettings["Destino_SALARIO_BASE"];

            if (!_usarConfigPadrao)
            {
                CriarPasta(destino, "SALARIO_BASE");
                path = destino + "\\SALARIO_BASE";
            }

            using (StreamWriter sw = new StreamWriter(path + $"\\{nomeColaborador.ToUpper()}-{codColaborador}_SALARIO_BASE.csv"))
            {
                sw.Write(@"MES_ANO;VALOR;FGTS;FGTS_REC.;CONTRIBUICAO_SOCIAL;CONTRIBUICAO_SOCIAL_REC.");

                foreach (var colaborador in salario_base_csv)
                    sw.Write($"\n{colaborador.Data.ToString("MM/yyyy")};{colaborador.SalarioBase};N;N;N;N");

                sw.Close();
            }
        }
        private void ExportarINSS_Proc(int codColaborador, string nomeColaborador, string destino = null)
        {
            var valoresINSS_Proc = _colaboradores.Where(x => x.Codigo == codColaborador).OrderBy(o => o.Data).Select(s => new { s.Nome, s.INSS_PROC, s.Data });
            string path = ConfigurationManager.AppSettings["Destino_INSS_Proc"];

            if (!_usarConfigPadrao)
            {
                CriarPasta(destino, "INSS_Proc");
                path = destino + "\\INSS_Proc";
            }

            using (StreamWriter sw = new StreamWriter(path + $"\\{nomeColaborador.ToUpper()}-{codColaborador}_INSS_Proc.csv"))
            {
                sw.Write(@"MES_ANO;VALOR;FGTS;FGTS_REC.;CONTRIBUICAO_SOCIAL;CONTRIBUICAO_SOCIAL_REC.");

                foreach (var colaborador in valoresINSS_Proc)
                    sw.Write($"\n{colaborador.Data.ToString("MM/yyyy")};{colaborador.INSS_PROC};N;N;S;S");

                sw.Close();
            }
        }
        private void ExportarInsalubridade(int codColaborador, string nomeColaborador, string destino = null)
        {
            var valoresInsalubridade = _colaboradores.Where(x => x.Codigo == codColaborador).OrderBy(o => o.Data).Select(s => new { s.Nome, s.InsalubridadeValor, s.Data });
            string path = ConfigurationManager.AppSettings["Destino_Insalubridade"];

            if (!_usarConfigPadrao)
            {
                CriarPasta(destino, "Insalubridade");
                path = destino + "\\Insalubridade";
            }

            using (StreamWriter sw = new StreamWriter(path + $"\\{nomeColaborador.ToUpper()}-{codColaborador}_Insalubridade.csv"))
            {
                sw.Write(@"MES_ANO;VALOR;FGTS;FGTS_REC.;CONTRIBUICAO_SOCIAL;CONTRIBUICAO_SOCIAL_REC.");

                foreach (var colaborador in valoresInsalubridade)
                    sw.Write($"\n{colaborador.Data.ToString("MM/yyyy")};{colaborador.InsalubridadeValor};N;N;N;N");

                sw.Close();
            }
        }
        private void ExportarPericulosidade(int codColaborador, string nomeColaborador, string destino = null)
        {
            var valoresPericulosidade = _colaboradores.Where(x => x.Codigo == codColaborador).OrderBy(o => o.Data).Select(s => new { s.Nome, s.PericulosidadeValor, s.Data });
            string path = ConfigurationManager.AppSettings["Destino_Periculosidade"];

            if (!_usarConfigPadrao)
            {
                CriarPasta(destino, "Periculosidade");
                path = destino + "\\Periculosidade";
            }

            using (StreamWriter sw = new StreamWriter(path + $"\\{nomeColaborador.ToUpper()}-{codColaborador}_Periculosidade.csv"))
            {
                sw.Write(@"MES_ANO;VALOR;FGTS;FGTS_REC.;CONTRIBUICAO_SOCIAL;CONTRIBUICAO_SOCIAL_REC.");

                foreach (var colaborador in valoresPericulosidade)
                    sw.Write($"\n{colaborador.Data.ToString("MM/yyyy")};{colaborador.PericulosidadeValor};N;N;N;N");

                sw.Close();
            }
        }
        private void CriarPasta(string folder, string pasta)
        {
            new DirectoryInfo(folder).CreateSubdirectory(pasta);
        }
    }
}
