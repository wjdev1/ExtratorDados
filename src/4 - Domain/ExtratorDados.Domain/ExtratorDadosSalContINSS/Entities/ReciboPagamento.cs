using System;

namespace ExtratorDados.Domain.ExtratorDadosSalContINSS.Entities
{
    public class ReciboPagamento
    {
        public decimal INSS_PROC { get; set; }
        public DateTime Data { get; set; }
        public bool IsDecimoTerceiro { get; set; }
    }
}
