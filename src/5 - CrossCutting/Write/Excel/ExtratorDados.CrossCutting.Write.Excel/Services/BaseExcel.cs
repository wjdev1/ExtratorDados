using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExtratorDados.CrossCutting.Write.Excel.Services
{
    public abstract class BaseExcel
    {
        protected virtual byte[] GerarXLSX<T>(List<T> list) where T : class
        {
            using (ExcelPackage ep = new ExcelPackage())
            {
                var sheet = ep.Workbook.Worksheets.Add("Planilha Totais");

                sheet.Cells.LoadFromCollection(list, true);

                return ep.GetAsByteArray();
            }
        }

        protected virtual void GerarXLSX<T>(List<T> list, string destino) where T : class
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage ep = new ExcelPackage(new FileInfo(destino)))
                {

                    var sheet = ep.Workbook.Worksheets.Add("Planilha Totais");

                    sheet.Cells.LoadFromCollection(list, true);
                    FormatColumns(sheet);
                    sheet.Cells.AutoFitColumns();
                    ep.Save();
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected virtual void FormatColumns(ExcelWorksheet worksheet) { }    
    
    }
}
