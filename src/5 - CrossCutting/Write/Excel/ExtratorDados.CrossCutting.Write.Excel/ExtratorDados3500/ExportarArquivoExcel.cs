using ExtratorDados.Domain.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using static ExtratorDados.Domain.Services.BaseService;

namespace ExtratorDados.CrossCutting.Write.Excel.ExtratorDados3500
{
    public class ExportarArquivoExcel
    {
        public event OnNotificador _onNotificar;

        public byte[] GerarXLSX(List<Colaborador> colaboradores)
        {
            var destino = ConfigurationManager.AppSettings["Destino_Planilha"] + $"PlanilhaCompleta_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx";

            using (ExcelPackage ep = new ExcelPackage(new FileInfo(destino)))
            {
                var sheet = ep.Workbook.Worksheets.Add("Planilha Totais");

                sheet.Cells.LoadFromCollection(colaboradores, true);

                return ep.GetAsByteArray();
            }
        }

        public void GerarXLSX(List<Colaborador> colaboradores, string destino)
        {
            _onNotificar.Invoke(string.Format(Domain.Resources.Notificacao.exportando_arquivo__0_, "excel"), null, false);

            try
            {
                if (destino == null)
                    destino = ConfigurationManager.AppSettings["Destino_Planilha"] + $"PlanilhaCompleta_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx";
                else
                    destino = destino + $"\\PlanilhaCompleta_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx";

                using (ExcelPackage ep = new ExcelPackage(new FileInfo(destino)))
                {
                    var sheet = ep.Workbook.Worksheets.Add("Planilha Totais");

                    sheet.Cells.LoadFromCollection(colaboradores, true);
                    sheet.Cells["N:N"].Clear();
                    ep.Save();
                }
            }
            catch (Exception ex)
            {
                _onNotificar.Invoke($"Ocorreu um erro: {ex.Message}", null, false, error: true);
            }
        }

    }
}
