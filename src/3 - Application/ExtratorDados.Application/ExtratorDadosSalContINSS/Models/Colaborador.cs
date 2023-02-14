using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtratorDados.Application.ExtratorDadosSalContINSS.Models
{
    public class Colaborador
    {
        [DisplayName("Nome Funcionário")]
        public string NomeFuncionario { get; set; }

        [DisplayName("Função Funcionário")]
        public string FuncaoFuncionario { get; set; }

        [DisplayName("Mês / Ano")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime MesAno { get; set; }

        [DisplayName("Sal. Contr. INSS")]
        public decimal SalContrINSS { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(NomeFuncionario))
                return false;
            else
            {
                ReplaceNome();
                RemoverSobreNomeAbreviado();
            }

            if (MesAno == DateTime.MinValue)
                return false;

            return true;
        }

        private void ReplaceNome()
        {
            var nomesInvalidos = new List<string>
            {
               "REBOBINADEIRA",
               "SECAO ANALISE"   ,
               "CORTE/SOLDA 49 HUDSON WICKETER",
               "IMPRESSORA 29-WINDMOLLER",
               "13 SERVIMEC" ,
               "IMPRESSORA 17 SERVIMEC 6 CORES",
               "IMPR.UN.II",
               "IMPRESSORA 17 SERVIMEC 6 CORES",
               "NOGUEIRA  S",
               "EXTRUSORA PE/PP 33 GLOUCESTER",
               "EXTRUSORA PP 30",
               "EXTRUSORA PE 27",
               "EXTRUSORA PE 32",
               "EXTRUSAO S",
               "EXTRUSORA PP 23" ,
               "IMPRESSORA 28 UTECO 8 CORES",
               "SUPERVISORES EXTRUSAO",
               "IMPRESSORA 24 UTECO 6 CORES",
               "14 SERVIMEC",
               "IMPRESSORA 23 UTECO 6 CORES",
               "IMPRESSORA 24 UTECO 6 CORES",
               "CORTE/SOLDA 48 HUDSON WICKETER",
               "PRE-",
               "CORTE/SOLDA 45 STAND POUCH 750",
               "CORTE/SOLDA 40 HECE 700",
               "MAQ CORTE SOLDA FUNDO COLADO 1",
               "CORTE/SOLDA 48 HUDSON WICKETER",
               "CORTE/SOLDA 34 FMC 1000",
               "CORTE/SOLDA 37 HECE SC-700 II",
               "CORTE E SOLDA",
               "CORTE SOLDA PET FOOD",
               "CORTE SOLDA HIGIENICOS",
               "LAMINACAO PET FOOD",
               "LAMINADORA 02 RAINBOW 120",
               "MELHORIA CONTINUA",
               "CORTE SOLDA-APOIO PROD HIGIEN.",
               "CORTE/SOLDA 31 FMC 1000" ,
               "IMPRESSORA 22 UTEC0 8 CORES" ,
               "LABORATORIO HIGIENICO",
               "IMPRESSORA 30 MPW",
               "SERVICOS GERAIS",
               "IMPRESSAO GOFRADO",
               "51 PETFOOD",
               "16 SERVIMEC",
               "SETUP",
               "SEGUR.PATRIMONIAL",
               "RECUPERACAO OPERACIONAL",
               "CONTROLE DE TARUGOS",
               "PET FOOD",
               "18 SERVIMEC",
               "QUALIDADE ASSEGURADA",
               "QUALIDADE PETFOOD",
               "11 SERVIMEC",
               "CORTE/SOLDA 31 FMC 1000",
               "PRE-SETUP",
               "EXTRUSAO",
               "GOFRADO",
               "PET FOOD",
               "EXPEDICAO",
               "ALMOXARIFADO",
               "LABORATORIO",
               "IMPRESSAO",
               "LAMINACAO",
               "MANUTENCAO S",
               "MANUTENCAO",
               "PROGRAMACAO",
               "HIGIENICO",
               "CORTE/SOLDA 42 HETTLER FK 2000",
               "EXTRUSORA PE 26",
               "SUPRIDORES",
               "BRAMBILLA",
               "RECEPCAO COMERCIAL",
               "CORPORATIVA",
               "IMPRESSORA 21 UTECO 6 CORES",
               "COORDENADOR",
               "CLICHERIA/FOTOLITO"
            };

            nomesInvalidos.ForEach(nome => 
            {
                if (NomeFuncionario.ToUpper().Contains(nome))
                    NomeFuncionario = NomeFuncionario.ReplaceInsensitive(nome, "");
            });

        }

        private void RemoverSobreNomeAbreviado()
        {
            var nomes = NomeFuncionario.Split(' ');

            NomeFuncionario = nomes.Last().Length == 1 && nomes.Last().ToUpper().Equals("S") ?
                NomeFuncionario.Substring(0, NomeFuncionario.Length - 2).TrimEnd() : NomeFuncionario;
        }
    }
}

static public class StringExtensions
{
    static public string ReplaceInsensitive(this string str, string from, string to)
    {
        str = Regex.Replace(str, from, to, RegexOptions.IgnoreCase);
        return str.TrimEnd();
    }
}
