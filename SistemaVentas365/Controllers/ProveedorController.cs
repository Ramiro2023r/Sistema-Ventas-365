using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaVentas365.Models;
using System.Data;

namespace SistemaVentas365.Controllers
{
    public class ProveedorController : Controller
    {


        //almacenamos el contenido del appsettings
        private readonly IConfiguration _config;
        public ProveedorController(IConfiguration config)
        {

            _config = config;

        }





        //Codigo para retornar los registros de clientes 
        IEnumerable<Proveedor> ListProveedores()
        {
            List<Proveedor> temporal = new List<Proveedor>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("exec usp_listar_proveedor", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    temporal.Add(new Proveedor()
                    {
                        Id = dr.GetInt32(0),
                        Ruc = dr.GetString(1),
                        Nombre = dr.GetString(2),
                        Telefono = dr.GetString(3),
                        Direccion = dr.GetString(4),
                        Fecha = dr.GetDateTime(5),
                        Estado = dr.GetInt32(6),
                    });
                }
                dr.Close();
            }
            return temporal;
        }




        //creamos la lista con paginacion 
        public async Task<IActionResult> ListPaProveedores(int p)
        {
            // Declaramos las variables para realizar la paginación

            // Número de registros por página
            int nr = 6;

            // Total de registros
            int tr = ListProveedores().Count();

            // Cálculo del número de páginas
            int paginas = tr % nr > 0 ? tr / nr + 1 : tr / nr;

            // Enviamos la página a la vista
            ViewBag.Paginas = paginas;

            // Retornamos el listado paginado
            return View(await Task.Run(() => ListProveedores().Skip(p * nr).Take(nr).ToList()));
        }






        public IActionResult Filtrar(string nombre, int p = 0)
        {
            IEnumerable<Proveedor> proveedores;

            if (string.IsNullOrEmpty(nombre))
            {
                // Si el nombre está vacío, mostrar todos los clientes
                proveedores = ListProveedores();
            }
            else
            {
                // Si se proporciona un nombre, filtrar los clientes por ese nombre
                proveedores = filtro_proveedores(nombre);
            }

            // Número de registros por página
            int nr = 6;

            // Aplicar paginación
            proveedores = proveedores.Skip(p * nr).Take(nr);

            // Calcular el número de páginas después de aplicar la paginación
            int tr = proveedores.Count(); // Total de registros después de aplicar la paginación
            int paginas = tr % nr > 0 ? tr / nr + 1 : tr / nr; // Cálculo del número de páginas

            // Verificar si el número de página es válido
            if (p < 0 || p >= paginas)
            {
                // Si el número de página no es válido, redireccionar a la página predeterminada
                return RedirectToAction("Filtrar", new { nombre = nombre, p = 0 });
            }

            // Pasar el número de páginas a la vista
            ViewBag.Paginas = paginas;

            // Pasar la lista de clientes paginada a la vista
            return View(proveedores);
        }




        //este es el metodo donde se ejecuta el porcedimiengto almacenado y busca por nombre

        IEnumerable<Proveedor> filtro_proveedores(string nombre)
        {
            List<Proveedor> temporal = new List<Proveedor>();

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_filtrar_proveedores", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Verificar si el nombre es nulo o vacío
                if (string.IsNullOrEmpty(nombre))
                {
                    // Si el nombre es nulo o vacío, no pasamos el parámetro @nom al procedimiento almacenado
                    // Esto hará que el procedimiento almacenado devuelva todos los clientes
                }
                else
                {
                    cmd.Parameters.AddWithValue("@nom", nombre);
                }

                SqlDataReader dr = cmd.ExecuteReader();

                // Verificar si no se encontraron registros
                if (!dr.HasRows)
                {
                    // Si no se encontraron registros, agregamos un cliente ficticio con un mensaje especial
                    temporal.Add(new Proveedor()
                    {
                        Nombre = "No se encontraron coincidencias para el nombre proporcionado."
                    });
                }
                else
                {
                    // Si se encontraron registros, los agregamos a la lista temporal como de costumbre
                    while (dr.Read())
                    {
                        temporal.Add(new Proveedor()
                        {
                            Id = dr.GetInt32(0),
                            Ruc = dr.GetString(1),
                            Nombre = dr.GetString(2),
                            Telefono = dr.GetString(3),
                            Direccion = dr.GetString(4),
                            Fecha = dr.GetDateTime(5),
                            Estado = dr.GetInt32(6),
                        });
                    }
                }

                dr.Close();
            }
            return temporal;
        }




        [HttpGet]
        public IActionResult CreatePro()
        {
            // Crear un nuevo objeto Proveedor con los valores predeterminados para Estado y Fecha
            Proveedor nuevoProveedor = new Proveedor
            {
                Fecha = DateTime.Now, // Fecha actual como valor predeterminado
                Estado = 1 // Valor predeterminado para Estado
            };

            // Pasar el objeto Proveedor a la vista
            return View(nuevoProveedor);
        }

        [HttpPost]
        public IActionResult CreatePro(Proveedor nuevoProveedor)
        {
            string mensaje = "";
            try
            {
                if (ModelState.IsValid) // Realizar validación de modelo
                {
                    // Guardar el nuevo proveedor en la base de datos
                    using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
                    {
                        SqlCommand cmd = new SqlCommand("sp_registrar_Proveedores", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ruc", nuevoProveedor.Ruc);
                        cmd.Parameters.AddWithValue("@nom", nuevoProveedor.Nombre);
                        cmd.Parameters.AddWithValue("@tele", nuevoProveedor.Telefono);
                        cmd.Parameters.AddWithValue("@direc", nuevoProveedor.Direccion);
                        cmd.Parameters.AddWithValue("@fecha", nuevoProveedor.Fecha);
                        cmd.Parameters.AddWithValue("@est", nuevoProveedor.Estado);
                        cn.Open();
                        int c = cmd.ExecuteNonQuery();
                        mensaje = $"Registro Insertado {c} en Base de Datos";
                    }

                    // Redireccionar a la acción para mostrar la lista de proveedores
                    return RedirectToAction("ListPaProveedores", "Proveedor", new { p = 0 });
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
            }

            // Si hay errores o el modelo no es válido, devolver la vista con el proveedor
            ViewBag.Mensaje = mensaje;
            return View(nuevoProveedor);
        }




        [HttpGet, ActionName("Delete")]
        public IActionResult DeletePro(int id)
        {
            string mensaje = "";

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_proveedores", cn);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //aperturamos la bd 
                cn.Open();
                //agregamos los respectivos parametros
                cmd.Parameters.AddWithValue("@id", id);
                //realizamos la ejecucion del procedimiento ala ...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro eliminado{c} de Proveedor";






            }//fin del using conexion bd 
            ViewBag.mensaje = mensaje;
            //redireccionamos hacia el listado 
            return RedirectToAction("ListPaProveedores");



        }




        //metodo esditar 
        //todos estos metodo se usan para editar un registro


        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Obtener los detalles del registro por su ID
            Proveedor model = ObtenerRegistroPorId(id);

            // Verificar si se encontró el registro
            if (model == null)
            {
                // Si no se encuentra el registro, devolver una respuesta de not found
                return NotFound();
            }

            // Si se encuentra el registro, pasar los detalles a la vista de edición
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(Proveedor model)
        {
            // Verificar si el modelo es válido
            if (!ModelState.IsValid)
            {
                return View(model); // Devolver la vista de edición con errores de validación si el modelo no es válido
            }

            try
            {
                // Actualizar el registro en la base de datos
                ActualizarRegistro(model);

                // Redirigir a la lista de registros después de la edición exitosa
                return RedirectToAction("ListPaProveedores");
            }
            catch (Exception ex)
            {
                // Manejar errores y devolver la vista de error si ocurre algún problema
                ViewBag.ErrorMessage = "Ocurrió un error al editar el registro.";
                return View("Error");
            }
        }

        ///incluimos el metod agregar detalle
        public IActionResult VerDetalle(int id)
        {
            // Obtener los detalles del cliente por su ID
            Proveedor cliente = ObtenerRegistroPorId(id);

            // Verificar si se encontró el cliente
            if (cliente == null)
            {
                // Si no se encuentra el cliente, devolver una respuesta de no encontrado
                return NotFound();
            }

            // Pasar los detalles del cliente a la vista para mostrarlos
            return View(cliente);
        }



        private Proveedor ObtenerRegistroPorId(int id)
        {
            // Declarar una instancia de ClassCalcular para almacenar los detalles del registro
            Proveedor model = null;

            // Conectar a la base de datos y recuperar los detalles del registro por su ID
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_buscar_proveedores", cn); // Suponiendo que tienes un procedimiento almacenado para buscar por ID
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                // Ejecutar el comando y leer los detalles del registro
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // Crear una nueva instancia de ClassCalcular y llenar sus propiedades con los datos del registro
                    model = new Proveedor()
                    {
                        Id = dr.GetInt32(0),
                        Ruc = dr.GetString(1),
                        Nombre = dr.GetString(2),
                        Telefono = dr.GetString(3),
                        Direccion = dr.GetString(4),
                        Fecha = dr.GetDateTime(5),
                        Estado = dr.GetInt32(6),
                    };
                }
            }

            // Devolver los detalles del registro recuperados
            return model;
        }

        private void ActualizarRegistro(Proveedor model)
        {
            // Calcular CapaCdMB y TotalCds nuevamente según el algoritmo


            // Actualizar el registro en la base de datos utilizando el procedimiento almacenado
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_actualizar_proveedor", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@ruc", model.Ruc);
                cmd.Parameters.AddWithValue("@nom", model.Nombre);
                cmd.Parameters.AddWithValue("@tele", model.Telefono);
                cmd.Parameters.AddWithValue("@direc", model.Direccion);
                cmd.Parameters.AddWithValue("@fecha", model.Fecha);
                cmd.Parameters.AddWithValue("@est", model.Estado);
                cmd.ExecuteNonQuery();
            }
        }



















        public IActionResult Index()
        {
            return View();
        }
    }
}
