using System;
using System.ComponentModel;

namespace ExtratorDados.Application.Models
{
    public class Colaborador
    {
        public int Codigo { get; set; }
        [DisplayName("Colaborador")]
        public string Nome { get; set; }
        public string Cargo { get; set; }

        private DateTime _MesAno;
        [DisplayName("MÊS/ANO")]
        public string MesAno => _MesAno.ToString("MM/yyyy");
        [DisplayName("Salário Base")]
        public decimal SalarioBase { get; set; }
        [DisplayName("INSS Proc")]
        public decimal INSS_PROC { get; set; }
        [DisplayName("Cod. 01 HS Normais Refer.")]
        public decimal HS_Normais { get; set; }
        [DisplayName("Cod. 12 Hs Férias Referência")]
        public decimal HS_Ferias_Referencia { get; set; }
        [DisplayName("Cod. 28 Referencia (Horas Aux.Doença INSS)")]
        public decimal HS_Aux_Doenca { get; set; }
        [DisplayName("Cod. 56 Hs Afast. Refer.")]
        public decimal HS_Afas_Refer { get; set; }
        [DisplayName("Cod. 62 Hs Insalubridade Valor")]
        public decimal InsalubridadeValor { get; set; }
        [DisplayName("Cod. 64 Per Paga Valor")]
        public decimal PericulosidadeValor { get; set; }
        public string Demissao { get; set; }
        public DateTime Data { get; set; }

        public void SetMesAno(DateTime mesAno)
        {
            this._MesAno = mesAno;
        }
    }
}
