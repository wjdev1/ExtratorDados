using System;

namespace ExtratorDados.Domain.Entities
{
    public class Colaborador
    {
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        private DateTime _MesAno;
        public string MesAno => _MesAno.ToString("MM/yyyy");
        public decimal SalarioBase { get; set; }
        public decimal INSS_PROC { get; set; }
        public decimal HS_Normais { get; set; }
        public decimal HS_Ferias_Referencia { get; set; }
        public decimal HS_Aux_Doenca { get; set; }
        public decimal HS_Afas_Refer { get; set; }
        public decimal InsalubridadeValor { get; set; }
        public decimal PericulosidadeValor { get; set; }
        public string Demissao { get; set; }
        public DateTime Data { get; set; }

        public void SetMesAno(DateTime mesAno)
        {
            this._MesAno = mesAno;
        }
    }

}
