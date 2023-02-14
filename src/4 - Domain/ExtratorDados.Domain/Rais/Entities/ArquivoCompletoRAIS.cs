using System;

namespace ExtratorDados.Domain.Rais.Entities
{
    public class ArquivoCompletoRAIS
    {
        public int Ano { get; set; }
        public string Nome { get; set; }
        public string PIS { get; set; }
        public string CPF { get; set; }
        public DateTime DataAdmissao { get; set; }
        public DateTime? DataDeslig { get; set; }
        public int? CodCausaDeslig { get; set; }
        public string MesAno { get; set; }
        public bool CompNaoLocalizada { get; set; }
        public bool MesesNaoDepositados { get; set; }
        public decimal Remuneracao { get; set; }
        public int ordem { get; set; }
    }
}
