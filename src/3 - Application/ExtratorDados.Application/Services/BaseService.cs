using System;

namespace ExtratorDados.Application.Services
{
    public abstract class BaseService
    {
        protected void OnNotificar(string mensagem, decimal? progress, bool progressBar, bool error = false)
        {
            Console.WriteLine(mensagem);
        }
    }
}
