namespace SistemaVentas365.Models
{
    public class Categoria
    {

        public int Idcategoria { get; set; }

        public string NombreC { get; set; } = null!;

        public DateTime Fecha { get; set; }

        public int Estado { get; set; }
    }
}
