using Dapper;
using Dapper.Contrib.Extensions;
using ExtratorDados.Domain.Entities;
using System;
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

        public Domain.Rais.Entities.Colaborador ObterColaboradorPorPISeAdmissao(string pis, DateTime dtAdmissao)
        {
            return connection.Query<Domain.Rais.Entities.Colaborador>("SELECT * FROM [RAIS].[dbo].[ColaboradorRAIS] WHERE replace(PIS,'.','') = @pis AND DataAdmissao = @dtAdm", new { pis, dtAdm = dtAdmissao })
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

        public async Task<IEnumerable<Domain.Rais.Entities.ArquivoCompletoRAIS>> ObterDados()
        {
            return await connection.QueryAsync<Domain.Rais.Entities.ArquivoCompletoRAIS>(sql: "EXEC RAIS..sp_RAIS_ObterRelatorio");
        }
    }
}
