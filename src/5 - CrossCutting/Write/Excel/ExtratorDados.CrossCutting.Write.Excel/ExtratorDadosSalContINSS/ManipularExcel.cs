using ExtratorDados.CrossCutting.Write.Excel.Services;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExtratorDados.CrossCutting.Write.Excel.ExtratorDadosSalContINSS
{
    public class ManipularExcel : BaseExcel
    {
        Dictionary<string, int> _dicValuePlanilha = new Dictionary<string, int>();

        public ManipularExcel()
        {
            SetDadosDictionaryValuePlaniha();
        }

        public void ExportarExcel(List<Domain.ExtratorDadosSalContINSS.Entities.Colaborador> colaboradores, string destino) 
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage ep = new ExcelPackage(new FileInfo(destino)))
                {

                    var sheet = ep.Workbook.Worksheets.Add("Planilha Totais");
                    int linha = 2;

                    sheet.Cells[linha, 1].Value = "Nome Funcionário";
                    sheet.Cells[linha, 2].Value = "Mês/ Ano";
                    sheet.Cells[linha, 3].Value = "Sal. Contr. INSS";

                    colaboradores.ForEach(colaborador => 
                    {
                        colaborador.ReciboPagamentos.ForEach(recibo => 
                        {
                            sheet.Cells[linha, 1].Value = colaborador.NomeCompleto;
                            sheet.Cells[linha, 2].Value = recibo.IsDecimoTerceiro ? "13/" + recibo.Data.Year : recibo.Data;
                            sheet.Cells[linha, 3].Value = recibo.INSS_PROC;
                            linha++;
                        });
                    });

                    sheet.Cells["B:B"].Style.Numberformat.Format = "MM/yyyy";
                    sheet.Cells.AutoFitColumns();
                    ep.Save();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void PreencherDadosNoExcel(string path, IEnumerable<Domain.ExtratorDadosSalContINSS.Entities.Colaborador> colaboradores)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Arquivo excel não existe!");

            try
            {
                using (ExcelPackage ep = new ExcelPackage(new System.IO.FileInfo(path)))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (FileStream stream = new FileStream(path, FileMode.Open))
                    {
                        ep.Load(stream);

                        foreach (var colaborador in colaboradores)
                        {
                            ep.Workbook.Worksheets.Copy("AD.PER.", $"{NomeAbaAdicionalPericulosidade(colaborador.NomeAbreviado)}");
                            ep.Workbook.Worksheets.Copy("INSS", $"{NomeAbaINSS(colaborador.NomeAbreviado)}");
                            
                            ExcelWorksheet planilhaADPericulosidade = ep.Workbook.Worksheets[$"{NomeAbaAdicionalPericulosidade(colaborador.NomeAbreviado)}"];
                            ExcelWorksheet planilhaINSS = ep.Workbook.Worksheets[$"{NomeAbaINSS(colaborador.NomeAbreviado)}"];

                            planilhaADPericulosidade.Cells[2, 1].Value = colaborador.NomeCompleto;
                            planilhaINSS.Cells[2, 1].Value = colaborador.NomeCompleto;

                            AlterarFormulaPlanilha(planilhaADPericulosidade, planilhaINSS);

                            foreach (var recibo in colaborador.ReciboPagamentos)
                            {
                                var mesAno = recibo.IsDecimoTerceiro ? $"13/{recibo.Data.ToString("yyyy")}" : recibo.Data.ToString("MM/yyyy");

                                var localeLinha = _dicValuePlanilha.GetValueOrDefault(mesAno);

                                if (localeLinha > 0)
                                {
                                    planilhaINSS.Cells[localeLinha, 3].Value = recibo.INSS_PROC;
                                }
                            }
                        }
                    }

                    Byte[] bin = ep.GetAsByteArray();
                    File.WriteAllBytes($@"C:\Teste\ExtratorNovo\DadosGerados\DadosGerados.xlsx", bin);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void SetDadosDictionaryValuePlaniha()
        {
            _dicValuePlanilha.Add("09/1998", 70);
            _dicValuePlanilha.Add("10/1998", 71);
            _dicValuePlanilha.Add("11/1998", 72);
            _dicValuePlanilha.Add("12/1998", 73);
            _dicValuePlanilha.Add("13/1998", 74);
            _dicValuePlanilha.Add("01/1999", 75);
            _dicValuePlanilha.Add("02/1999", 76);
            _dicValuePlanilha.Add("03/1999", 77);
            _dicValuePlanilha.Add("04/1999", 78);
            _dicValuePlanilha.Add("05/1999", 79);
            _dicValuePlanilha.Add("06/1999", 80);
            _dicValuePlanilha.Add("07/1999", 81);
            _dicValuePlanilha.Add("08/1999", 82);
            _dicValuePlanilha.Add("09/1999", 83);
            _dicValuePlanilha.Add("10/1999", 84);
            _dicValuePlanilha.Add("11/1999", 85);
            _dicValuePlanilha.Add("12/1999", 86);
            _dicValuePlanilha.Add("13/1999", 87);
            _dicValuePlanilha.Add("01/2000", 88);
            _dicValuePlanilha.Add("02/2000", 89);
            _dicValuePlanilha.Add("03/2000", 90);
            _dicValuePlanilha.Add("04/2000", 91);
            _dicValuePlanilha.Add("05/2000", 92);
            _dicValuePlanilha.Add("06/2000", 93);
            _dicValuePlanilha.Add("07/2000", 94);
            _dicValuePlanilha.Add("08/2000", 95);
            _dicValuePlanilha.Add("09/2000", 96);
            _dicValuePlanilha.Add("10/2000", 97);
            _dicValuePlanilha.Add("11/2000", 98);
            _dicValuePlanilha.Add("12/2000", 99);
            _dicValuePlanilha.Add("13/2000", 100);
            _dicValuePlanilha.Add("01/2001", 101);
            _dicValuePlanilha.Add("02/2001", 102);
            _dicValuePlanilha.Add("03/2001", 103);
            _dicValuePlanilha.Add("04/2001", 104);
            _dicValuePlanilha.Add("05/2001", 105);
            _dicValuePlanilha.Add("06/2001", 106);
            _dicValuePlanilha.Add("07/2001", 107);
            _dicValuePlanilha.Add("08/2001", 108);
            _dicValuePlanilha.Add("09/2001", 109);
            _dicValuePlanilha.Add("10/2001", 110);
            _dicValuePlanilha.Add("11/2001", 111);
            _dicValuePlanilha.Add("12/2001", 112);
            _dicValuePlanilha.Add("13/2001", 113);
            _dicValuePlanilha.Add("01/2002", 114);
            _dicValuePlanilha.Add("02/2002", 115);
            _dicValuePlanilha.Add("03/2002", 116);
            _dicValuePlanilha.Add("04/2002", 117);
            _dicValuePlanilha.Add("05/2002", 118);
            _dicValuePlanilha.Add("06/2002", 119);
            _dicValuePlanilha.Add("07/2002", 120);
            _dicValuePlanilha.Add("08/2002", 121);
            _dicValuePlanilha.Add("09/2002", 122);
            _dicValuePlanilha.Add("10/2002", 123);
            _dicValuePlanilha.Add("11/2002", 124);
            _dicValuePlanilha.Add("12/2002", 125);
            _dicValuePlanilha.Add("13/2002", 126);
            _dicValuePlanilha.Add("01/2003", 127);
            _dicValuePlanilha.Add("02/2003", 128);
            _dicValuePlanilha.Add("03/2003", 129);
            _dicValuePlanilha.Add("04/2003", 130);
            _dicValuePlanilha.Add("05/2003", 131);
            _dicValuePlanilha.Add("06/2003", 132);
            _dicValuePlanilha.Add("07/2003", 133);
            _dicValuePlanilha.Add("08/2003", 134);
            _dicValuePlanilha.Add("09/2003", 135);
            _dicValuePlanilha.Add("10/2003", 136);
            _dicValuePlanilha.Add("11/2003", 137);
            _dicValuePlanilha.Add("12/2003", 138);
            _dicValuePlanilha.Add("13/2003", 139);
            _dicValuePlanilha.Add("01/2004", 140);
            _dicValuePlanilha.Add("02/2004", 141);
            _dicValuePlanilha.Add("03/2004", 142);
            _dicValuePlanilha.Add("04/2004", 143);
            _dicValuePlanilha.Add("05/2004", 144);
            _dicValuePlanilha.Add("06/2004", 145);
            _dicValuePlanilha.Add("07/2004", 146);
            _dicValuePlanilha.Add("08/2004", 147);
            _dicValuePlanilha.Add("09/2004", 148);
            _dicValuePlanilha.Add("10/2004", 149);
            _dicValuePlanilha.Add("11/2004", 150);
            _dicValuePlanilha.Add("12/2004", 151);
            _dicValuePlanilha.Add("13/2004", 152);
            _dicValuePlanilha.Add("01/2005", 153);
            _dicValuePlanilha.Add("02/2005", 154);
            _dicValuePlanilha.Add("03/2005", 155);
            _dicValuePlanilha.Add("04/2005", 156);
            _dicValuePlanilha.Add("05/2005", 157);
            _dicValuePlanilha.Add("06/2005", 158);
            _dicValuePlanilha.Add("07/2005", 159);
            _dicValuePlanilha.Add("08/2005", 160);
            _dicValuePlanilha.Add("09/2005", 161);
            _dicValuePlanilha.Add("10/2005", 162);
            _dicValuePlanilha.Add("11/2005", 163);
            _dicValuePlanilha.Add("12/2005", 164);
            _dicValuePlanilha.Add("13/2005", 165);
            _dicValuePlanilha.Add("01/2006", 166);
            _dicValuePlanilha.Add("02/2006", 167);
            _dicValuePlanilha.Add("03/2006", 168);
            _dicValuePlanilha.Add("04/2006", 169);
            _dicValuePlanilha.Add("05/2006", 170);
            _dicValuePlanilha.Add("06/2006", 171);
            _dicValuePlanilha.Add("07/2006", 172);
            _dicValuePlanilha.Add("08/2006", 173);
            _dicValuePlanilha.Add("09/2006", 174);
            _dicValuePlanilha.Add("10/2006", 175);
            _dicValuePlanilha.Add("11/2006", 176);
            _dicValuePlanilha.Add("12/2006", 177);
            _dicValuePlanilha.Add("13/2006", 178);
            _dicValuePlanilha.Add("01/2007", 179);
            _dicValuePlanilha.Add("02/2007", 180);
            _dicValuePlanilha.Add("03/2007", 181);
            _dicValuePlanilha.Add("04/2007", 182);
            _dicValuePlanilha.Add("05/2007", 183);
            _dicValuePlanilha.Add("06/2007", 184);
            _dicValuePlanilha.Add("07/2007", 185);
            _dicValuePlanilha.Add("08/2007", 186);
            _dicValuePlanilha.Add("09/2007", 187);
            _dicValuePlanilha.Add("10/2007", 188);
            _dicValuePlanilha.Add("11/2007", 189);
            _dicValuePlanilha.Add("12/2007", 190);
            _dicValuePlanilha.Add("13/2007", 191);
            _dicValuePlanilha.Add("01/2008", 192);
            _dicValuePlanilha.Add("02/2008", 193);
            _dicValuePlanilha.Add("03/2008", 194);
            _dicValuePlanilha.Add("04/2008", 195);
            _dicValuePlanilha.Add("05/2008", 196);
            _dicValuePlanilha.Add("06/2008", 197);
            _dicValuePlanilha.Add("07/2008", 198);
            _dicValuePlanilha.Add("08/2008", 199);
            _dicValuePlanilha.Add("09/2008", 200);
            _dicValuePlanilha.Add("10/2008", 201);
            _dicValuePlanilha.Add("11/2008", 202);
            _dicValuePlanilha.Add("12/2008", 203);
            _dicValuePlanilha.Add("13/2008", 204);
            _dicValuePlanilha.Add("01/2009", 205);
            _dicValuePlanilha.Add("02/2009", 206);
            _dicValuePlanilha.Add("03/2009", 207);
            _dicValuePlanilha.Add("04/2009", 208);
            _dicValuePlanilha.Add("05/2009", 209);
            _dicValuePlanilha.Add("06/2009", 210);
            _dicValuePlanilha.Add("07/2009", 211);
            _dicValuePlanilha.Add("08/2009", 212);
            _dicValuePlanilha.Add("09/2009", 213);
            _dicValuePlanilha.Add("10/2009", 214);
            _dicValuePlanilha.Add("11/2009", 215);
            _dicValuePlanilha.Add("12/2009", 216);
            _dicValuePlanilha.Add("13/2009", 217);
            _dicValuePlanilha.Add("01/2010", 218);
            _dicValuePlanilha.Add("02/2010", 219);
            _dicValuePlanilha.Add("03/2010", 220);
            _dicValuePlanilha.Add("04/2010", 221);
            _dicValuePlanilha.Add("05/2010", 222);
            _dicValuePlanilha.Add("06/2010", 223);
            _dicValuePlanilha.Add("07/2010", 224);
            _dicValuePlanilha.Add("08/2010", 225);
            _dicValuePlanilha.Add("09/2010", 226);
            _dicValuePlanilha.Add("10/2010", 227);
            _dicValuePlanilha.Add("11/2010", 228);
            _dicValuePlanilha.Add("12/2010", 229);
            _dicValuePlanilha.Add("13/2010", 230);
            _dicValuePlanilha.Add("01/2011", 231);
            _dicValuePlanilha.Add("02/2011", 232);
            _dicValuePlanilha.Add("03/2011", 233);
            _dicValuePlanilha.Add("04/2011", 234);
            _dicValuePlanilha.Add("05/2011", 235);
            _dicValuePlanilha.Add("06/2011", 236);
            _dicValuePlanilha.Add("07/2011", 237);
            _dicValuePlanilha.Add("08/2011", 238);
            _dicValuePlanilha.Add("09/2011", 239);
            _dicValuePlanilha.Add("10/2011", 240);
            _dicValuePlanilha.Add("11/2011", 241);
            _dicValuePlanilha.Add("12/2011", 242);
            _dicValuePlanilha.Add("13/2011", 243);
            _dicValuePlanilha.Add("01/2012", 244);
            _dicValuePlanilha.Add("02/2012", 245);
            _dicValuePlanilha.Add("03/2012", 246);
            _dicValuePlanilha.Add("04/2012", 247);
            _dicValuePlanilha.Add("05/2012", 248);
            _dicValuePlanilha.Add("06/2012", 249);
            _dicValuePlanilha.Add("07/2012", 250);
            _dicValuePlanilha.Add("08/2012", 251);
            _dicValuePlanilha.Add("09/2012", 252);
            _dicValuePlanilha.Add("10/2012", 253);
            _dicValuePlanilha.Add("11/2012", 254);
            _dicValuePlanilha.Add("12/2012", 255);
            _dicValuePlanilha.Add("13/2012", 256);
            _dicValuePlanilha.Add("01/2013", 257);
            _dicValuePlanilha.Add("02/2013", 258);
            _dicValuePlanilha.Add("03/2013", 259);
            _dicValuePlanilha.Add("04/2013", 260);
            _dicValuePlanilha.Add("05/2013", 261);
            _dicValuePlanilha.Add("06/2013", 262);
            _dicValuePlanilha.Add("07/2013", 263);
            _dicValuePlanilha.Add("08/2013", 264);
            _dicValuePlanilha.Add("09/2013", 265);
            _dicValuePlanilha.Add("10/2013", 266);
            _dicValuePlanilha.Add("11/2013", 267);
            _dicValuePlanilha.Add("12/2013", 268);
            _dicValuePlanilha.Add("13/2013", 269);
            _dicValuePlanilha.Add("01/2014", 270);
            _dicValuePlanilha.Add("02/2014", 271);
            _dicValuePlanilha.Add("03/2014", 272);
            _dicValuePlanilha.Add("04/2014", 273);
            _dicValuePlanilha.Add("05/2014", 274);
            _dicValuePlanilha.Add("06/2014", 275);
            _dicValuePlanilha.Add("07/2014", 276);
            _dicValuePlanilha.Add("08/2014", 277);
            _dicValuePlanilha.Add("09/2014", 278);
            _dicValuePlanilha.Add("10/2014", 279);
            _dicValuePlanilha.Add("11/2014", 280);
            _dicValuePlanilha.Add("12/2014", 281);
            _dicValuePlanilha.Add("13/2014", 282);
            _dicValuePlanilha.Add("01/2015", 283);
            _dicValuePlanilha.Add("02/2015", 284);
            _dicValuePlanilha.Add("03/2015", 285);
            _dicValuePlanilha.Add("04/2015", 286);

        }

        private string NomeAbaAdicionalPericulosidade(string nomeColaboradorAbreviado)
        {
            return (nomeColaboradorAbreviado + "- AD.PER.").Length <= 31 ?
                                nomeColaboradorAbreviado + "- AD.PER." :
                                nomeColaboradorAbreviado.Substring(0, 15).TrimEnd() + "- AD.PER."; 
        }

        private string NomeAbaINSS(string nomeColaboradorAbreviado)
        {
            return (nomeColaboradorAbreviado + "- INSS").Length <= 31 ?
                                nomeColaboradorAbreviado + "- INSS" :
                                nomeColaboradorAbreviado.Substring(0, 15).TrimEnd() + "- INSS";
        }

        private void AlterarFormulaPlanilha(ExcelWorksheet worksheet, ExcelWorksheet worksheetReferenciaAba)
        {
            for (int i = 4; i < 221; i++)
            {
                worksheet.Cells[i, 2].Formula = worksheet.Cells[i, 2].Formula.Replace("INSS", $"'{worksheetReferenciaAba.Name}'").Replace("AD.PER.", worksheet.Name);
            }            
        }
    }
}
