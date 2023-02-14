using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtratorDados.Domain.ExtratorDadosSalContINSS.Entities
{
    public class Colaborador
    {
        public string NomeCompleto { get; set; }
        public List<ReciboPagamento> ReciboPagamentos { get; set; } = new List<ReciboPagamento>();

        public void GerarDecimoTerceiro()
        {
            var anosAgrupados = this.ReciboPagamentos.GroupBy(x => x.Data.Year).Select(x => new { Ano = x.Key, Meses = x, Valor = x.Sum(s => s.INSS_PROC) });
            var primeiroDecimoTerceiro = true;

            foreach (var ano in anosAgrupados)
            {
                var meses = ano.Meses.Count();
                var ultimoMesPago = this.ReciboPagamentos.Where(x => x.Data.Year == ano.Ano).Select(s => s.Data.Month).LastOrDefault();
                var ultimoValorPago = this.ReciboPagamentos.Where(x => x.Data.Year == ano.Ano).Select(s => s.INSS_PROC).LastOrDefault();

                if (primeiroDecimoTerceiro)
                    ultimoMesPago = ano.Meses.Count();

                if (!primeiroDecimoTerceiro && ultimoMesPago != 12 && anosAgrupados.Any(x => x.Ano == ano.Ano + 1))
                    ultimoMesPago = 12;

                this.ReciboPagamentos.Add(new Domain.ExtratorDadosSalContINSS.Entities.ReciboPagamento
                {
                    Data = new DateTime(ano.Ano, 12, 1),
                    IsDecimoTerceiro = true,
                    INSS_PROC = ultimoValorPago / 12 * ultimoMesPago
                });

                primeiroDecimoTerceiro = false;
            }
        }

        public string NomeAbreviado => AbreviarNome(NomeCompleto);

        public string AbreviarNome(string nome)
        {
            var sobreNome = " ";
            var nomes = nome.Split(' ');
            for (var i = 1; i < nomes.Length; i++)
            {
                if (!nomes[i].Equals("de", StringComparison.OrdinalIgnoreCase) &&
                    !nomes[i].Equals("da", StringComparison.OrdinalIgnoreCase) &&
                    !nomes[i].Equals("do", StringComparison.OrdinalIgnoreCase) &&
                    !nomes[i].Equals("das", StringComparison.OrdinalIgnoreCase) &&
                    !nomes[i].Equals("dos", StringComparison.OrdinalIgnoreCase))
                    sobreNome += nomes[i][0] + ". ";
            }
            return nomes[0] + sobreNome;
        }
    }

}
