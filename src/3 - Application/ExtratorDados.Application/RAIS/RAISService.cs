using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtratorDados.Application.RAIS
{
    public class RAISService
    {
        CrossCutting.Read.PDF.Rais.RAISLeituraPDFService _service = new CrossCutting.Read.PDF.Rais.RAISLeituraPDFService();
        public async Task Processar(string pathRAIS, string pathExtrato)
        {

        }
    }
}
