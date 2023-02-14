using Dapper.Contrib.Extensions;
using System.Data.SqlClient;

namespace ExtratorDados.CrossCutting.Data.RAIS
{
    public class RAISRepositorio
    {
        private readonly SqlConnection connection;

        public RAISRepositorio()
        {
            connection = new SqlConnection("");
        }

        public async Task AddAsync(Domain.Rais.Entities.Colaborador colaborador)
        {          
            await connection.InsertAsync(colaborador);
        } 
    }
}
