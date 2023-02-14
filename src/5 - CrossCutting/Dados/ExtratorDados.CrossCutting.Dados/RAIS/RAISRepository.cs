using Dapper;
using Dapper.Contrib.Extensions;
using ExtratorDados.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ExtratorDados.CrossCutting.Dados.RAIS
{
    public class RAISRepository<T> where T : class
    {
        public IDbConnection connection;

        public RAISRepository()
        {
            connection = new SqlConnection("Data Source=.;Initial Catalog=RAIS;Integrated Security=True;");
        }

        public async Task AddAsync(T entity)
        {
            await connection.InsertAsync(entity);
        }

        public void Add(T entity)
        {
            try
            {
                connection.Insert(entity);
            }
            catch (System.Exception ex)
            {
                var a = entity;
            }
        }

        public Domain.Rais.Entities.Colaborador ObterColaboradorPorPIS(string pis)
        {
            return connection.Query<Domain.Rais.Entities.Colaborador>("SELECT * FROM [RAIS].[dbo].[ColaboradorRAIS] WHERE replace(PIS,'.','') = @pis", new { pis })
                .FirstOrDefault();
        }
    }

    public class Repository 
    {
        public IDbConnection connection;

        public Repository()
        {
            connection = new SqlConnection("Data Source=.;Initial Catalog=RAIS;Integrated Security=True;");
        }

        public Domain.Rais.Entities.Colaborador ObterColaboradorPorPIS(string pis)
        {
            return connection.Query<Domain.Rais.Entities.Colaborador>("SELECT * FROM [RAIS].[dbo].[ColaboradorRAIS] WHERE replace(PIS,'.','') = @pis", new { pis })
                .FirstOrDefault();
        }

        public IEnumerable<Domain.Rais.Entities.ArquivoCompletoRAIS> ObterDados()
        {
            var sb = new System.Text.StringBuilder(2845);
sb.AppendLine(@"WITH DADOS AS(");
sb.AppendLine(@"SELECT distinct");
sb.AppendLine(@"YEAR(rem.DataPagamento) as Ano,");
sb.AppendLine(@"Nome,PIS,rais.CPF,DataAdmissao,");
sb.AppendLine(@"DataDesligamento,");
sb.AppendLine(@"CodCausaDesligamento,");
sb.AppendLine(@"CONVERT(varchar(10),MONTH(rem.DataPagamento)) + '/' + CONVERT(varchar(10),YEAR(rem.DataPagamento)) 'Mes/Ano',");
sb.AppendLine(@"'CompNaoLocalizada' = (SELECT DISTINCT 1 FROM RAIS..CompNaoLocalizada WHERE Ano = YEAR(rem.DataPagamento) ");
sb.AppendLine(@"AND MesAno = REM.DataPagamento AND PIS = RAIS.PIS),");
sb.AppendLine(@"'MesesNaoDepositados' = (SELECT distinct 1 ");
sb.AppendLine(@"FROM RAIS..ColaboradorRAIS r ");
sb.AppendLine(@"WHERE r.CPF = rais.CPF and r.Ano = rais.Ano and ");
sb.AppendLine(@"exists(SELECT 1 FROM RAIS..ExtratoFGTS e WHERE E.CPF = RAIS.CPF AND E.Data = REM.DataPagamento)),");
sb.AppendLine(@"rem.ValorPagamento 'Remuneracao',");
sb.AppendLine(@"REM.DataPagamento,");
sb.AppendLine(@"MONTH(REM.DataPagamento) 'ordem'");
sb.AppendLine(@"FROM RAIS..ColaboradorRAIS rais");
sb.AppendLine(@"INNER JOIN RAIS..Remuneracao rem on rem.CPFColaborador = rais.CPF");
sb.AppendLine(@"AND (YEAR(rem.DataPagamento) <= YEAR(rais.DataDesligamento)");
sb.AppendLine(@"AND MONTH(rem.DataPagamento) <= MONTH(rais.DataDesligamento)");
sb.AppendLine(@"OR rais.DataDesligamento is null)");
sb.AppendLine(@"AND rem.DataPagamento >= '2011-09-01'");
sb.AppendLine(@"AND rem.DataPagamento <= '2016-09-30'");
sb.AppendLine(@"WHERE rem.ValorPagamento > 0 AND rais.Ano = YEAR(rem.DataPagamento)");
sb.AppendLine(@"UNION --13º adiantamento");
sb.AppendLine(@"SELECT distinct");
sb.AppendLine(@"deci.Ano,");
sb.AppendLine(@"Nome,PIS,rais.CPF,");
sb.AppendLine(@"deci.DataAdmissao,");
sb.AppendLine(@"deci.DataDesligamento,");
sb.AppendLine(@"deci.CodCausaDesligamento,");
sb.AppendLine(@"'',");
sb.AppendLine(@"'',");
sb.AppendLine(@"'',");
sb.AppendLine(@"sum(DECI.DecTerceiroAdiantamento) 'Remuneracao',");
sb.AppendLine(@"CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',");
sb.AppendLine(@"14 'ordem'");
sb.AppendLine(@"FROM RAIS..ColaboradorRAIS rais");
sb.AppendLine(@"INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF");
sb.AppendLine(@"WHERE DECI.DecTerceiroAdiantamento > 0");
sb.AppendLine(@"group by Deci.Ano,RAIS.Nome,RAIS.PIS,RAIS.CPF,deci.DataAdmissao,deci.DataDesligamento,deci.CodCausaDesligamento");
sb.AppendLine(@"UNION --13º parcela final");
sb.AppendLine(@"SELECT distinct");
sb.AppendLine(@"deci.Ano,");
sb.AppendLine(@"Nome,PIS,rais.CPF,'','',");
sb.AppendLine(@"'',");
sb.AppendLine(@"'',");
sb.AppendLine(@"'',");
sb.AppendLine(@"'',");
sb.AppendLine(@"sum(DECI.DecTerceiroParcFinal) 'Remuneracao',");
sb.AppendLine(@"CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',");
sb.AppendLine(@"15 'ordem'");
sb.AppendLine(@"FROM RAIS..ColaboradorRAIS rais");
sb.AppendLine(@"INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF");
sb.AppendLine(@"WHERE DECI.DecTerceiroParcFinal > 0");
sb.AppendLine(@"group by Deci.Ano,RAIS.Nome,RAIS.PIS,RAIS.CPF");
sb.AppendLine(@"UNION --aviso prévio");
sb.AppendLine(@"SELECT distinct");
sb.AppendLine(@"deci.Ano,");
sb.AppendLine(@"Nome,PIS,rais.CPF,");
sb.AppendLine(@"deci.DataAdmissao,");
sb.AppendLine(@"deci.DataDesligamento,");
sb.AppendLine(@"deci.CodCausaDesligamento,");
sb.AppendLine(@"CONVERT(varchar(2),12) + '/' + CONVERT(varchar(4),deci.Ano) 'Mes/Ano',");
sb.AppendLine(@"'',");
sb.AppendLine(@"'',");
sb.AppendLine(@"DECI.AvisoPrevio 'Remuneracao',");
sb.AppendLine(@"CAST(CAST(deci.Ano AS varchar) + '-' + CAST('12' AS varchar) + '-' + CAST('01' AS varchar) AS DATE) as 'DataPagamento',");
sb.AppendLine(@"16 'ordem'");
sb.AppendLine(@"FROM RAIS..ColaboradorRAIS rais");
sb.AppendLine(@"INNER JOIN RAIS..DecimoTerceiroEAviso DECI ON DECI.Ano = RAIS.Ano AND DECI.CPF = rais.CPF");
sb.AppendLine(@"WHERE DECI.AvisoPrevio > 0");
sb.AppendLine(@")");
sb.AppendLine(@"SELECT ");
sb.AppendLine(@"*");
sb.AppendLine(@"FROM DADOS ");
sb.AppendLine(@"ORDER BY Nome,DataPagamento,ordem");


            return connection.Query<Domain.Rais.Entities.ArquivoCompletoRAIS>(sql: sb.ToString());
        }
    }
}
