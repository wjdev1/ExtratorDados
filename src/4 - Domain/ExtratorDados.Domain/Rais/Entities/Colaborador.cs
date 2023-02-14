using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ExtratorDados.Domain.Rais.Entities
{
    [Table("dbo.ColaboradorRAIS")]
    public class Colaborador
    {
        protected Colaborador() { }
        public Colaborador(int ano, string nome, string pIS, string cPF, DateTime dataAdmissao, DateTime? dataDesligamento, int? codCausaDesligamento)
        {
            Ano = ano;
            Nome = nome;
            PIS = pIS;
            CPF = cPF;
            DataAdmissao = dataAdmissao;
            DataDesligamento = dataDesligamento;
            CodCausaDesligamento = codCausaDesligamento;
            Remuneracoes = new List<Remuneracao>();

            Valid();

        }

        public int Ano { get; set; }
        public string Nome { get; set; }
        public string PIS { get; set; }
        public string CPF { get; set; }
        public DateTime DataAdmissao { get; set; }
        public DateTime? DataDesligamento { get; set; }
        public int? CodCausaDesligamento { get; set; }

        [Write(false)]
        public List<Remuneracao> Remuneracoes { get; set; }         

        private void Valid()
        {
            if (Ano == 0) throw new ArgumentException("Ano inválido");
            if (string.IsNullOrEmpty(Nome)) throw new ArgumentException("Nome inválido");
            if (string.IsNullOrEmpty(PIS)) throw new ArgumentException("PIS inválido");
            if (string.IsNullOrEmpty(CPF)) throw new ArgumentException("CPF inválido");
            if (DataAdmissao == DateTime.MinValue) throw new ArgumentException("Data Admissão inválida");
        }
    }

    [Table("dbo.Remuneracao")]
    public class Remuneracao
    {
        public string CPFColaborador { get; set; }
        public DateTime DataPagamento { get; set; }
        public decimal ValorPagamento { get; set; }
    }
}
