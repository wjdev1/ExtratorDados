namespace ExtratorDados.Domain.Services
{
    public abstract class BaseService
    {
        public delegate void OnNotificador(string mensagem, decimal? progress, bool progressBar, bool error = false);
        public event OnNotificador _onNotificar;
    }
}
