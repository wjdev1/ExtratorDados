using System;

namespace ExtratorDados.Domain.Rais.Entities
{
    public class ArquivoCompletoRAIS
    {
        public int Ano { get; set; }
        public string Nome { get; set; }
        public string PIS { get; set; }
        public string CPF { get; set; }
        public string DataAdmissao { get; set; }
        public string DataDesligamento { get; set; }
        public int? CodCausaDeslig { get; set; }
        public string MesAno { get; set; }
        public string CompNaoLocalizada { get; set; }
        public string MesesNaoDepositados { get; set; }
        public decimal Remuneracao { get; set; }
    }
}
