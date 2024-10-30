using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas365.Models
{
    public class Productos
    {

        public int Id { get; set; }
        public string Foto { get; set;  }
      

        public string CodigoBarras { get; set; }

        public string Descripcion { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal Cantidad { get; set; }

        [Display(Name = "Selecciona Medida")]
        public int Idmedida { get; set; }

        [Display(Name = "NombreMedida")]
        public string NombreM { get; set; }


        [Display(Name = "Selecciona Categoria")]
        public int Idcategoria { get; set; }

        [Display(Name = "NombreCategoria")]
        public string NombreC { get; set; }


        public int Estado { get; set; }

        public DateTime Fecha { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}
