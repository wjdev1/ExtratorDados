using Dapper.Contrib.Extensions;
using System;

namespace ExtratorDados.Domain.Rais.Entities
{
    [Table("dbo.CompNaoLocalizada")]
    public class CompNaoLocalizada
    {
        public string PIS { get; set; }
        public DateTime MesAno { get; set; }
    }

    [Table("dbo.ExtratoFGTS")]
    public class ExtratoFGTS
    {
        public string CPF { get; set; }
        public string Descricao { get; set; }
        public DateTime  Data { get; set; }
        public decimal Valor { get; set; }
    }
}
