using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CardMarket.Models
{
    public class Producto
    {
        public int Id { get; set; }
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La descripción es un campo requerido.")]
        public string? Descripcion { get; set; }
        public string? Texto { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }
        [Display(Name = "Precio")]
        [RegularExpression(@"^[-0123456789]+[0-9.,]*$",
        ErrorMessage = "El valor introducido debe ser de tipo monetario.")]
        [Required(ErrorMessage = "El precio es un campo requerido")]
        public string PrecioCadena
        {
            get
            {
                return Convert.ToString(Precio).Replace(',', '.');
            }
            set
            {
                Precio = Convert.ToDecimal(value.Replace('.', ','));
            }
        }
        public int? Stock { get; set; }
        public bool? Escaparate { get; set; }
        public string? Imagen { get; set; }
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        public ICollection<Detalle>? Detalles { get; set; }
    }
}
