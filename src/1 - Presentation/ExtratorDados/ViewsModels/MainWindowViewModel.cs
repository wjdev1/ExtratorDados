using ExtratorDados.Application.Services;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace ExtratorDados.ViewsModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogCoordinator _dialog;
        private ProgressDialogController progressDialog;
        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialog = dialogCoordinator;
        }

        #region Commands
        private DelegateCommand _selecionarArquivoPDFCommand;
        public DelegateCommand SelecionarArquivoPDFCommand =>
            _selecionarArquivoPDFCommand ?? (_selecionarArquivoPDFCommand = new DelegateCommand(SelecionarArquivoPDF));

        private DelegateCommand _selecionarDestinoCommand;
        public DelegateCommand SelecionarDestinoCommand =>
            _selecionarDestinoCommand ?? (_selecionarDestinoCommand = new DelegateCommand(SelecionarDestino));

        private DelegateCommand _processarExtracaoCommand;
        public DelegateCommand ProcessarExtracaoCommand =>
            _processarExtracaoCommand ?? (_processarExtracaoCommand =
            new DelegateCommand(ExecutarProcessamento, () =>
            !string.IsNullOrEmpty(_arquivoPDF) && !string.IsNullOrEmpty(_destino))).
            ObservesProperty(() => Destino).
            ObservesProperty(() => ArquivoPDF);

        //ProcessarExtracaoCommand
        #endregion

        #region PROPERTIES
        private string _arquivoPDF;
        public string ArquivoPDF
        {
            get { return _arquivoPDF; }
            set { SetProperty(ref _arquivoPDF, value); }
        }

        private string _destino;
        public string Destino
        {
            get { return _destino; }
            set { SetProperty(ref _destino, value); }
        }
        #endregion

        #region METHODS
        private void SelecionarArquivoPDF()
        {
            try
            {
                VistaOpenFileDialog dialog = new VistaOpenFileDialog();
                dialog.Filter = "arquivo pdf (*.pdf*)|*.pdf*";
                if ((bool)dialog.ShowDialog())
                    ArquivoPDF = dialog.FileName;
            }
            catch (Exception ex)
            {
                _dialog.ShowMessageAsync(this, "Ocorreu um erro", ex.Message);
            }
        }

        private void SelecionarDestino()
        {
            try
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Selecione a pasta";
                dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

                if ((bool)dialog.ShowDialog())
                {
                    Destino = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                _dialog.ShowMessageAsync(this, "Ocorreu um erro", ex.Message);
            }
        }

        private async void ExecutarProcessamento()
        {
            try
            {
                progressDialog = await _dialog.ShowProgressAsync(this, "Aguarde...", "Processando");
                progressDialog.SetIndeterminate();

                var processando = Task.Factory.StartNew(() =>
                {
                    var processar = new ProcessarArquivoPDFAppService();
                    processar._onNotificar += Notificando;

                    processar.IniciarProcesso(_arquivoPDF, _destino);
                });

                await processando;
                await progressDialog?.CloseAsync();

                await _dialog.ShowMessageAsync(this, "Informação", "Processo finalizado com sucesso!");
            }
            catch (Exception ex)
            {
                await _dialog.ShowMessageAsync(this, "Ocorreu um erro", ex.Message);
            }
        }
        private void Notificando(string notificacao, decimal? progressStatus, bool progressBar, bool error = false)
        {

            if (progressStatus != null && progressBar)
            {
                var progresso = Math.Round((double)progressStatus * 100, 2);
                progressDialog.SetMessage(notificacao + " " + progresso.ToString() + " %");
                progressDialog.SetProgress((double)progressStatus);
            }
            else if (progressStatus != null)
            {
                var progresso = Math.Round((double)progressStatus / 100, 2);
                progressDialog.SetProgress(progresso);
            }
            else if (error)
            {
                _dialog.ShowMessageAsync(this, "Erro", notificacao);
            }
            else
            {
                progressDialog.SetIndeterminate();
                progressDialog.SetMessage(notificacao);
            }
        }

        #endregion

    }

}
