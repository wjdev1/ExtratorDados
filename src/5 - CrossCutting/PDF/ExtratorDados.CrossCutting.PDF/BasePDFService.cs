using System;
using System.IO;
using System.Threading.Tasks;

namespace ExtratorDados.CrossCutting.PDF
{
    public abstract class BasePDFService
    {
        #region Atributos
        protected readonly string _path;
        #endregion

        #region Construtor
        public BasePDFService(string path)
        {
            _path = path;
        }
        #endregion

        #region Métodos       
        public virtual Task LimparPasta()
        {
            try
            {
                var files = Directory.GetFiles(_path);

                foreach (var file in files)
                    File.Delete(file);
            }
            catch (Exception)
            {

            }

            return Task.CompletedTask;
        }

        protected virtual string[] ObterArquivos() =>
            Directory.GetFiles(_path, "*.pdf");
        #endregion
    }
}
