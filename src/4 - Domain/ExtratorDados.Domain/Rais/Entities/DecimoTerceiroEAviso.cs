using Dapper.Contrib.Extensions;
using System;

namespace ExtratorDados.Domain.Rais.Entities
{
    [Table("dbo.DecimoTerceiroEAviso")]
    public class DecimoTerceiroEAviso
    {
        public string CPF { get; set; }
        public int Ano { get; set; }
        public DateTime DataAdmissao { get;set;}
        public DateTime? DataDesligamento { get; set; }
        public int? CodCausaDesligamento { get; set; }
        public decimal DecTerceiroAdiantamento { get; set; }
        public decimal DecTerceiroParcFinal { get; set; }
        public decimal AvisoPrevio { get; set; }
    }
}
