using Dapper.Contrib.Extensions;

namespace ExtratorDados.Domain.Rais.Entities
{
    [Table("dbo.Arquivo")]
    public class Arquivo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Texto { get; set; }
    }
}
