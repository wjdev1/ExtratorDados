using System.ComponentModel.DataAnnotations;

namespace ExtratorDados.WebMVC.Models
{
    public class ExtratorDadosSalContINSSViewModel
    {
        [Display(Name = "Caminho arquivos PDF")]
        [Required(ErrorMessage = "O Caminho arquivos PDF é obrigatório!")]
        public string? CaminhoPastaPDF { get; set; }

        [Display(Name = "Caminho Excel Modelo")]
        [Required(ErrorMessage = "O {0} é obrigatório!")]
        public string? CaminhoExcelModelo { get; set; }
    }
}
