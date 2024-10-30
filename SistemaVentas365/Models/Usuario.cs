using System;
using System.Collections.Generic;

namespace SistemaVentas365.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Usuario1 { get; set; } = null!;

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public int Tipo { get; set; }

    public DateTime Fecha { get; set; }

    public int Estado { get; set; }
}
