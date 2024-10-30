using Microsoft.AspNetCore.Mvc;

//referenciamks las librerias 
using Microsoft.Data.SqlClient;
using SistemaVentas365.Models;
using System.Data;
using System.Threading.Tasks;

namespace SistemaVentas365.Controllers
{
    public class ClientesController : Controller
    {

        //almacenamos el contenido del appsettings
        private readonly IConfiguration _config;
        public ClientesController(IConfiguration config)
        {

            _config = config;

        }


        //Codigo para retornar los registros de clientes 
        IEnumerable<Cliente> Listclientes()
        {
            List<Cliente> temporal = new List<Cliente>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("exec sp_listar_clientes", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    temporal.Add(new Cliente()
                    {
                        Id = dr.GetInt32(0),
                        Dni = dr.GetString(1),
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
        public async Task<IActionResult> ListPaClientes(int p)
        {
            // Declaramos las variables para realizar la paginación

            // Número de registros por página
            int nr = 6;

            // Total de registros
            int tr = Listclientes().Count();

            // Cálculo del número de páginas
            int paginas = tr % nr > 0 ? tr / nr + 1 : tr / nr;

            // Enviamos la página a la vista
            ViewBag.Paginas = paginas;

            // Retornamos el listado paginado
            return View(await Task.Run(() => Listclientes().Skip(p * nr).Take(nr).ToList()));
        }




        //metodo para buscar por nombre del clientes
        //creamos el metodo filtrar por nombre de cliente
        //este es el IActionResul donde se crea la vista

        public IActionResult Filtrar(string nombre, int p = 0)
        {
            IEnumerable<Cliente> clientes;

            if (string.IsNullOrEmpty(nombre))
            {
                // Si el nombre está vacío, mostrar todos los clientes
                clientes = Listclientes();
            }
            else
            {
                // Si se proporciona un nombre, filtrar los clientes por ese nombre
                clientes = filtro_clientes(nombre);
            }

            // Número de registros por página
            int nr = 6;

            // Aplicar paginación
            clientes = clientes.Skip(p * nr).Take(nr);

            // Calcular el número de páginas después de aplicar la paginación
            int tr = clientes.Count(); // Total de registros después de aplicar la paginación
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
            return View(clientes);
        }


        //este es el metodo donde se ejecuta el porcedimiengto almacenado y busca por nombre

        IEnumerable<Cliente> filtro_clientes(string nombre)
        {
            List<Cliente> temporal = new List<Cliente>();

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_clientes_buscar", cn);
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
                    temporal.Add(new Cliente()
                    {
                        Nombre = "No se encontraron coincidencias para el nombre proporcionado."
                    });
                }
                else
                {
                    // Si se encontraron registros, los agregamos a la lista temporal como de costumbre
                    while (dr.Read())
                    {
                        temporal.Add(new Cliente()
                        {
                            Id = dr.GetInt32(0),
                            Dni = dr.GetString(1),
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





        //este es IACtion result aqui se crea la vista y emos dicho que alos campos estado y fecha los agregue por defecto 

        public IActionResult AgregarCliente()
        {
            // Crear un nuevo objeto Cliente con los valores predeterminados para Estado y Fecha
            Cliente nuevoCliente = new Cliente
            {
                Estado = 1, // Aquí estableces el valor predeterminado para Estado
                Fecha = DateTime.Now // Aquí estableces la fecha actual como valor predeterminado para Fecha
            };

            // Pasar el objeto Cliente a la vista
            return View(nuevoCliente);
        }

        //este es el metodo agregar cliente nos permite agregar un cliente 
        //Metodo Post

        [HttpPost]
        public IActionResult AgregarCliente(Cliente objcotizar)
        {
            string mensaje = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
                {
                    SqlCommand cmd = new SqlCommand("sp_registrar_clientes", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@dni", objcotizar.Dni);
                    cmd.Parameters.AddWithValue("@nom", objcotizar.Nombre);
                    cmd.Parameters.AddWithValue("@tele", objcotizar.Telefono);
                    cmd.Parameters.AddWithValue("@direc", objcotizar.Direccion);
                    cmd.Parameters.AddWithValue("@fecha", objcotizar.Fecha);
                    cmd.Parameters.AddWithValue("@est", objcotizar.Estado);
                    cn.Open();
                    int c = cmd.ExecuteNonQuery();
                    mensaje = $"Registro Insertado {c} en Base de Datos";
                }
                // Redireccionar a ListPaClientes
                return RedirectToAction("ListPaClientes", "Clientes", new { p = 0 });
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
            }
            ViewBag.Mensaje = mensaje;
            return View(objcotizar);
        }








        //codigo donde se crea la lista 
        public async Task<IActionResult> Index()
        {
            return View(await Task.Run(() => Listclientes()));
        }

        //public async Task<IActionResult> Filtrar(string? nombre = null)
        //{
        //    return View(await Task.Run(() =>
        //    string.IsNullOrEmpty(nombre) ? new List<Cliente>() : filtro_clientes(nombre)));
        //}



        //[HttpGet]
        //public IActionResult Delete(int id)
        //{
        //    //instanciamos las respectivas clases 
        //    Cliente cot = new Cliente();
        //    using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cn"]))
        //    {

        //        SqlCommand cmd = new SqlCommand("sp_buscar_clientes @id", cn);

        //        cmd.Parameters.AddWithValue("@id", id);

        //        cn.Open();
        //        //recuperamos los datos de la base de datos y los almacenamos en las 
        //        //propiedades
        //        using (SqlDataReader dr = cmd.ExecuteReader())
        //        {
        //            if (dr.Read())
        //            {

        //                cot.Id = Convert.ToInt32(dr["id"]);
        //                cot.Dni = (string)dr["dni"];
        //                cot.Nombre = (string)dr["nombre"];
        //                cot.Telefono = (string)dr["telefono"];
        //                cot.Direccion = (string)dr["direccion"];
        //                cot.Fecha = (DateTime)dr["fecha"];
        //                cot.Estado = (int)dr["estado"];




        //            }


        //        }
        //        return View(cot);
        //    }

        //    //obtenemos la conexion 


        //}//fin del metodo  delete...



        //[HttpPost, ActionName("Delete")]
        [HttpGet, ActionName("Delete")]
        public IActionResult Deletecli(int id)
        {
            string mensaje = "";

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_clientes", cn);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //aperturamos la bd 
                cn.Open();
                //agregamos los respectivos parametros
                cmd.Parameters.AddWithValue("@id", id);
                //realizamos la ejecucion del procedimiento ala ...
                int c = cmd.ExecuteNonQuery();
                mensaje = $"registro eliminado{c} de Cliente";






            }//fin del using conexion bd 
            ViewBag.mensaje = mensaje;
            //redireccionamos hacia el listado 
            return RedirectToAction("ListPaClientes");



        }



        //metodo esditar 
        //todos estos metodo se usan para editar un registro


        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Obtener los detalles del registro por su ID
            Cliente model = ObtenerRegistroPorId(id);

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
        public IActionResult Edit(Cliente model)
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
                return RedirectToAction("ListPaClientes");
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
            Cliente cliente = ObtenerRegistroPorId(id);

            // Verificar si se encontró el cliente
            if (cliente == null)
            {
                // Si no se encuentra el cliente, devolver una respuesta de no encontrado
                return NotFound();
            }

            // Pasar los detalles del cliente a la vista para mostrarlos
            return View(cliente);
        }



        private Cliente ObtenerRegistroPorId(int id)
        {
            // Declarar una instancia de ClassCalcular para almacenar los detalles del registro
            Cliente model = null;

            // Conectar a la base de datos y recuperar los detalles del registro por su ID
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_buscar_clientes", cn); // Suponiendo que tienes un procedimiento almacenado para buscar por ID
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                // Ejecutar el comando y leer los detalles del registro
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // Crear una nueva instancia de ClassCalcular y llenar sus propiedades con los datos del registro
                    model = new Cliente()
                    {
                        Id = dr.GetInt32(0),
                        Dni = dr.GetString(1),
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

        private void ActualizarRegistro(Cliente model)
        {
            // Calcular CapaCdMB y TotalCds nuevamente según el algoritmo


            // Actualizar el registro en la base de datos utilizando el procedimiento almacenado
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("sp_actualizar_clientes", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@dni", model.Dni);
                cmd.Parameters.AddWithValue("@nom", model.Nombre);
                cmd.Parameters.AddWithValue("@tele", model.Telefono);
                cmd.Parameters.AddWithValue("@direc", model.Direccion);
                cmd.Parameters.AddWithValue("@fecha", model.Fecha);
                cmd.Parameters.AddWithValue("@est", model.Estado);
                cmd.ExecuteNonQuery();
            }
        }


    }




    ///agregamos el metodo ver datalles 
    ///
   



}
