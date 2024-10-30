namespace SistemaVentas365.Models
{
    public class Proveedores
    {

        public int Id { get; set; }
        public string Ruc {  get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public DateTime Fecha { get; set; }
        public int Estado { get; set; }

    }
}
