using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SistemaVentas365.Models;
using System.Data;

namespace SistemaVentas365.Controllers
{
    public class MedidasController : Controller
    {



        private readonly IConfiguration _config;
        public MedidasController(IConfiguration config)
        {

            _config = config;

        }


        /*Metodo listarr de categoria*/

        IEnumerable<Medida> Listmedidas()
        {
            List<Medida> temporal = new List<Medida>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("exec usp_listra_medidas", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    temporal.Add(new Medida()
                    {
                        Idmedida = dr.GetInt32(0),
                        NombreM = dr.GetString(1),
                        NombreCorto = dr.GetString(2),
                        Fecha = dr.GetDateTime(3),
                        Estado = dr.GetInt32(4)
                    });
                }
                dr.Close();
            }
            return temporal;
        }



        /*Metodo Paginacion de Categorias*/

        public async Task<IActionResult> ListPaMedidas(int p)
        {
            // Declaramos las variables para realizar la paginación

            // Número de registros por página
            int nr = 6;

            // Total de registros
            int tr = Listmedidas().Count();

            // Cálculo del número de páginas
            int paginas = tr % nr > 0 ? tr / nr + 1 : tr / nr;

            // Enviamos la página a la vista
            ViewBag.Paginas = paginas;

            // Retornamos el listado paginado
            return View(await Task.Run(() => Listmedidas().Skip(p * nr).Take(nr).ToList()));
        }

        public IActionResult Index()
        {
            return View();
        }





        /*metodo para filtarar */


        public IActionResult Filtrar(string nombre, int p = 0)
        {
            IEnumerable<Medida> medida;

            if (string.IsNullOrEmpty(nombre))
            {
                // Si el nombre está vacío, mostrar todos los clientes
                medida = Listmedidas();
            }
            else
            {
                // Si se proporciona un nombre, filtrar los clientes por ese nombre
                medida = filtro_categoria(nombre);
            }

            // Número de registros por página
            int nr = 6;

            // Aplicar paginación
            medida = medida.Skip(p * nr).Take(nr);

            // Calcular el número de páginas después de aplicar la paginación
            int tr = medida.Count(); // Total de registros después de aplicar la paginación
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
            return View(medida);
        }



        //este es el metodo donde se ejecuta el porcedimiengto almacenado y busca por nombre

        IEnumerable<Medida> filtro_categoria(string nombre)
        {
            List<Medida> temporal = new List<Medida>();

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_medidas_buscar", cn);
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
                    temporal.Add(new Medida()
                    {
                        NombreM = "No se encontraron coincidencias para el nombre proporcionado."
                    });
                }
                else
                {
                    // Si se encontraron registros, los agregamos a la lista temporal como de costumbre
                    while (dr.Read())
                    {
                        temporal.Add(new Medida()
                        {
                            Idmedida = dr.GetInt32(0),
                            NombreM = dr.GetString(1),
                            NombreCorto = dr.GetString(2),                        
                            Fecha = dr.GetDateTime(3),
                            Estado = dr.GetInt32(4),
                        });
                    }
                }

                dr.Close();
            }
            return temporal;
        }




        //este es IACtion result aqui se crea la vista y emos dicho que alos campos estado y fecha los agregue por defecto 

        public IActionResult AgregarMedida()
        {
            // Crear un nuevo objeto Cliente con los valores predeterminados para Estado y Fecha
            Medida nuevoMedidad = new Medida
            {
                Fecha = DateTime.Now, // Aquí estableces la fecha actual como valor predeterminado para Fecha
                Estado = 1 // Aquí estableces el valor predeterminado para Estado

            };

            // Pasar el objeto Cliente a la vista
            return View(nuevoMedidad);
        }




        //este es el metodo agregar categoria nos permite agregar un categoria 
        //Metodo Post

        [HttpPost]
        public IActionResult AgregarMedida(Medida objcotizar)
        {
            string mensaje = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
                {
                    SqlCommand cmd = new SqlCommand("sp_registrar_medidas", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nom", objcotizar.NombreM);
                    cmd.Parameters.AddWithValue("@nom_c", objcotizar.NombreCorto);
                    cmd.Parameters.AddWithValue("@fecha", objcotizar.Fecha);
                    cmd.Parameters.AddWithValue("@est", objcotizar.Estado);
                    cn.Open();
                    int c = cmd.ExecuteNonQuery();
                    mensaje = $"Registro Insertado {c} en Base de Datos";
                }
                // Redireccionar a ListPaClientes
                return RedirectToAction("ListPaMedidas", "Medidas", new { p = 0 });
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
            }
            ViewBag.Mensaje = mensaje;
            return View(objcotizar);
        }





        /*Metodo Eliminar c **/

        [HttpGet, ActionName("Delete")]
        public IActionResult Deletecate(int id)
        {
            string mensaje = "";

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_medidas", cn);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //aperturamos la bd 
                cn.Open();
                //agregamos los respectivos parametros
                cmd.Parameters.AddWithValue("@id", id);
                //realizamos la ejecucion del procedimiento ala ...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro eliminado{c} de Categoria";






            }//fin del using conexion bd 
            ViewBag.mensaje = mensaje;
            //redireccionamos hacia el listado 
            return RedirectToAction("ListPaMedidas");



        }






        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Obtener los detalles del registro por su ID
            Medida model = ObtenerRegistroPorId(id);

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
        public IActionResult Edit(  Medida model)
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
                return RedirectToAction("ListPaMedidas");
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
            Medida categoria = ObtenerRegistroPorId(id);

            // Verificar si se encontró el cliente
            if (categoria == null)
            {
                // Si no se encuentra el cliente, devolver una respuesta de no encontrado
                return NotFound();
            }

            // Pasar los detalles del cliente a la vista para mostrarlos
            return View(categoria);
        }



        private Medida ObtenerRegistroPorId(int id)
        {
            // Declarar una instancia de ClassCalcular para almacenar los detalles del registro
            Medida model = null;

            // Conectar a la base de datos y recuperar los detalles del registro por su ID
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_buscar_medidaporid", cn); // Suponiendo que tienes un procedimiento almacenado para buscar por ID
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                // Ejecutar el comando y leer los detalles del registro
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // Crear una nueva instancia de ClassCalcular y llenar sus propiedades con los datos del registro
                    model = new Medida()
                    {
                        Idmedida = dr.GetInt32(0),
                        NombreM = dr.GetString(1),
                        NombreCorto = dr.GetString(2),
                        Fecha = dr.GetDateTime(3),
                        Estado = dr.GetInt32(4),
                    };
                }
            }

            // Devolver los detalles del registro recuperados
            return model;
        }

        private void ActualizarRegistro(Medida model)
        {
            // Calcular CapaCdMB y TotalCds nuevamente según el algoritmo


            // Actualizar el registro en la base de datos utilizando el procedimiento almacenado
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_actualizar_medidas", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", model.Idmedida);
                cmd.Parameters.AddWithValue("@nom", model.NombreM);
                cmd.Parameters.AddWithValue("@nom_c", model.NombreCorto);
                cmd.Parameters.AddWithValue("@fecha", model.Fecha);
                cmd.Parameters.AddWithValue("@est", model.Estado);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
