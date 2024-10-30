namespace SistemaVentas365.Models
{
    public class Cliente
    {

        public int Id { get; set; }

        public string? Dni { get; set; }

        public string Nombre { get; set; } = null!;

        public string Telefono { get; set; } = null!;

        public string Direccion { get; set; } = null!;

        public DateTime Fecha { get; set; }

        public int Estado { get; set; }
    }
}
