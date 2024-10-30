using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//referencialmos alas librerias de firebase
using Firebase.Auth;
using Firebase.Storage;


using System.Data;
//referenciamks las librerias 
using Microsoft.Data.SqlClient;
using SistemaVentas365.Models;


using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace SistemaVentas365.Controllers
{
    public class ProductoController : Controller
    {

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _config = config;
            _webHostEnvironment = webHostEnvironment;
        }


        //private readonly IConfiguration _config;


        //public ProductoController(IConfiguration config)
        //{
        //    _config = config;

        //}


        IEnumerable<Productos> prod()
        {
            List<Productos> aut = new List<Productos>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_Product", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    aut.Add(new Productos
                    {

                        Id = dr.GetInt32(0),
                        Foto = dr.GetString(1),
                        CodigoBarras = dr.GetString(2),
                        Descripcion = dr.GetString(3),
                        PrecioCompra = dr.GetDecimal(4),
                        PrecioVenta = dr.GetDecimal(5),
                        Cantidad = dr.GetDecimal(6),
                        NombreM = dr.GetString(7),
                        Idmedida = dr.GetInt32(8),
                        NombreC = dr.GetString(9),
                        Idcategoria = dr.GetInt32(10),
                        Estado = dr.GetInt32(11),
                        Fecha = dr.GetDateTime(12),
                    });
                }



            }
            return aut;
        }






      


        //creamos la lista con paginacion 
        public async Task<IActionResult> ListPaProductos(int p)
        {
            // Declaramos las variables para realizar la paginación

            // Número de registros por página
            int nr = 6;

            // Total de registros
            int tr = prod().Count();

            // Cálculo del número de páginas
            int paginas = tr % nr > 0 ? tr / nr + 1 : tr / nr;

            // Enviamos la página a la vista
            ViewBag.Paginas = paginas;

            // Retornamos el listado paginado
            return View(await Task.Run(() => prod().Skip(p * nr).Take(nr).ToList()));
        }




        public IActionResult Filtrar(string nombre, int p = 0)
        {
            IEnumerable<Productos> productos;

            if (string.IsNullOrEmpty(nombre))
            {
                // Si el nombre está vacío, mostrar todos los clientes
                productos = prod();
            }
            else
            {
                // Si se proporciona un nombre, filtrar los clientes por ese nombre
                productos = filtro_productos(nombre);
            }

            // Número de registros por página
            int nr = 6;

            // Aplicar paginación
            productos = productos.Skip(p * nr).Take(nr);

            // Calcular el número de páginas después de aplicar la paginación
            int tr = productos.Count(); // Total de registros después de aplicar la paginación
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
            return View(productos);
        }




        IEnumerable<Productos> filtro_productos(string nombre)
        {
            List<Productos> temporal = new List<Productos>();

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_filtrar_productos", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Verificar si el nombre es nulo o vacío
                if (string.IsNullOrEmpty(nombre))
                {
                    // Si el nombre es nulo o vacío, no pasamos el parámetro @nom al procedimiento almacenado
                    // Esto hará que el procedimiento almacenado devuelva todos los clientes
                }
                else
                {
                    cmd.Parameters.AddWithValue("@descr", nombre);
                }

                SqlDataReader dr = cmd.ExecuteReader();

                // Verificar si no se encontraron registros
                if (!dr.HasRows)
                {
                    // Si no se encontraron registros, agregamos un cliente ficticio con un mensaje especial
                    temporal.Add(new Productos()
                    {
                        Descripcion = "No se encontraron coincidencias para el nombre proporcionado."
                    });
                }
                else
                {
                    // Si se encontraron registros, los agregamos a la lista temporal como de costumbre
                    while (dr.Read())
                    {
                        temporal.Add(new Productos()
                        {

                            Id = dr.GetInt32(0),
                            Foto = dr.GetString(1),
                            CodigoBarras = dr.GetString(2),
                            Descripcion = dr.GetString(3),
                            PrecioCompra = dr.GetDecimal(4),
                            PrecioVenta = dr.GetDecimal(5),
                            Cantidad = dr.GetDecimal(6),
                            Idmedida = dr.GetInt32(7),                      
                            Idcategoria = dr.GetInt32(8),
                            Estado = dr.GetInt32(9),
                            Fecha = dr.GetDateTime(10),
                        });
                    }
                }

                dr.Close();
            }
            return temporal;
        }

















        //public async Task<IActionResult> ListadoPro()
        //{
        //    return View(await Task.Run(() => prod()));
        //}

        IEnumerable<Medida> medidas()
        {
            List<Medida> medi = new List<Medida>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("exec usp_listra_medidas", cn);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    medi.Add(new Medida()
                    {
                        Idmedida = dr.GetInt32(0),
                        NombreM = dr.GetString(1),
                        NombreCorto = dr.GetString(2),
                        Fecha = dr.GetDateTime(3),
                        Estado = dr.GetInt32(4),
                    });
                }

            }
            return medi;
        }


        IEnumerable<Categoria> categorias()
        {
            List<Categoria> catego = new List<Categoria>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("exec usp_listar_categorias", cn);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    catego.Add(new Categoria()
                    {
                        Idcategoria = dr.GetInt32(0),
                        NombreC = dr.GetString(1),
                        Fecha = dr.GetDateTime(2),
                        Estado = dr.GetInt32(3),
                    });
                }

            }
            return catego;
        }


        Productos Buscar(int id)
        {
            Productos? reg = prod().Where(V => V.Id == id).FirstOrDefault();
            return reg;
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Productos reg = Buscar(id);
            //aplicamos una condicion
            if (reg == null) return RedirectToAction("ListPaProductos");
            ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM", reg.Idmedida);
            ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC", reg.Idcategoria);
            //retornamos la vista
            return View(reg);
        }//fin del metodo



        //[HttpPost]
        //public async Task<IActionResult> Edit(Productos model)
        //{
        //    //aplicamos una condicion
        //    if (!ModelState.IsValid)
        //    {

        //        ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM", model.Idmedida);
        //        ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC", model.Idcategoria);
        //        return View(model);

        //    }
        //    string mensaje = "";
        //    using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
        //    {
        //        cn.Open();
        //        SqlCommand cmd = new SqlCommand("usp_merge_productos", cn);
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@idpro", model.Id);
        //        cmd.Parameters.AddWithValue("@foto", model.Foto);
        //        cmd.Parameters.AddWithValue("@codi", model.CodigoBarras);
        //        cmd.Parameters.AddWithValue("@descr", model.Descripcion);
        //        cmd.Parameters.AddWithValue("@pcom", model.PrecioCompra);
        //        cmd.Parameters.AddWithValue("@pven", model.PrecioVenta);
        //        cmd.Parameters.AddWithValue("@can", model.Cantidad);
        //        cmd.Parameters.AddWithValue("@idme", model.Idmedida);
        //        cmd.Parameters.AddWithValue("@idca", model.Idcategoria);
        //        cmd.Parameters.AddWithValue("@est", model.Estado);
        //        cmd.Parameters.AddWithValue("@fech", model.Fecha);

        //        int c = cmd.ExecuteNonQuery();
        //        mensaje = $"Registro actualizado{c} de auto";

        //    }
        //    ViewBag.mensaje = mensaje;
        //    ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM", model.Idmedida);
        //    ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC", model.Idcategoria);
        //    return RedirectToAction("ListPaProductos", "Producto");
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(int id, Productos model, IFormFile Foto)
        //{
        //    if (id != model.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM", model.Idmedida);
        //        ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC", model.Idcategoria);
        //        return View(model);
        //    }

        //    string urlimagen = model.Foto;

        //    if (Foto != null)
        //    {
        //        Stream image = Foto.OpenReadStream();
        //        urlimagen = await SubirStorage(image, Foto.FileName);
        //    }

        //    using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
        //    {
        //        cn.Open();
        //        SqlCommand cmd = new SqlCommand("usp_merge_productos", cn);
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //        cmd.Parameters.AddWithValue("@idpro", model.Id);
        //        cmd.Parameters.AddWithValue("@foto", urlimagen);
        //        cmd.Parameters.AddWithValue("@codi", model.CodigoBarras);
        //        cmd.Parameters.AddWithValue("@descr", model.Descripcion);
        //        cmd.Parameters.AddWithValue("@pcom", model.PrecioCompra);
        //        cmd.Parameters.AddWithValue("@pven", model.PrecioVenta);
        //        cmd.Parameters.AddWithValue("@can", model.Cantidad);
        //        cmd.Parameters.AddWithValue("@idme", model.Idmedida);
        //        cmd.Parameters.AddWithValue("@idca", model.Idcategoria);
        //        cmd.Parameters.AddWithValue("@est", model.Estado);
        //        cmd.Parameters.AddWithValue("@fech", model.Fecha);

        //        int c = cmd.ExecuteNonQuery();
        //        if (c == 0)
        //        {
        //            return NotFound();
        //        }
        //    }

        //    return RedirectToAction("ListPaProductos", "Producto");
        //}








        [HttpGet]
        public async Task<ActionResult> Create()
        {

            ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM");
            ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC");
            Productos producto = new Productos
            {
                Estado = 1, // Aquí estableces el valor predeterminado para Estado
                Fecha = DateTime.Now // Aquí estableces la fecha actual como valor predeterminado para Fecha
            };
            return View(producto);
        }



        [HttpPost]
        public async Task<IActionResult> Create(Productos model, IFormFile Imagen)
        {
            if (ModelState.IsValid)
            {
                ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM", model);
                ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC", model);
                return View(model);


            }
            //RECIBIR LOS DATOS DEL FORMULARIO
            Stream image = Imagen.OpenReadStream();
            string urlimagen = await SubirStorage(image, Imagen.FileName);

            String mensaje = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_merge_productos", cn);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idpro", 0);
                cmd.Parameters.AddWithValue("@foto", urlimagen);
                cmd.Parameters.AddWithValue("@codi", model.CodigoBarras);
                cmd.Parameters.AddWithValue("@descr", model.Descripcion);
                cmd.Parameters.AddWithValue("@pcom", model.PrecioCompra);
                cmd.Parameters.AddWithValue("@pven", model.PrecioVenta);
                cmd.Parameters.AddWithValue("@can", model.Cantidad);
                cmd.Parameters.AddWithValue("@idme", model.Idmedida);
                cmd.Parameters.AddWithValue("@idca", model.Idcategoria);
                cmd.Parameters.AddWithValue("@est", model.Estado);
                cmd.Parameters.AddWithValue("@fech", model.Fecha);


                int c = cmd.ExecuteNonQuery();
                mensaje = $"Registro insertado{c} de Producto";

            }
            ViewBag.mensaje = mensaje;
            ViewBag.medidas = new SelectList(await Task.Run(() => medidas()), "Idmedida", "NombreM", model.Idmedida);
            ViewBag.categorias = new SelectList(await Task.Run(() => categorias()), "Idcategoria", "NombreC", model.Idcategoria);
            return RedirectToAction("ListPaProductos", "Producto");
        }




        [HttpGet, ActionName("Delete")]
        public IActionResult DeletePro(int id)
        {
            string mensaje = "";

            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_productos", cn);

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
            return RedirectToAction("ListPaProductos");



        }


        public IActionResult VerDetalle(int id)
        {
            // Obtener los detalles del cliente por su ID
            Productos producto = Buscar(id);

            // Verificar si se encontró el cliente
            if (producto == null)
            {
                // Si no se encuentra el cliente, devolver una respuesta de no encontrado
                return NotFound();
            }

            // Pasar los detalles del cliente a la vista para mostrarlos
            return View(producto);
        }


        //public IActionResult Index()
        //{
        //    return View();
        //}


        //metodo del coexion con firebase


        public async Task<string> SubirStorage(Stream archivo, string nombre)
        {

            //INGRESA AQUÍ TUS PROPIAS CREDENCIALES
            string email = "codigo@gmail.com";
            string clave = "codigo123";
            string ruta = "sistemagestionventasimg365.appspot.com";
            string api_key = "AIzaSyDsqmdSde9_JBMtbXBTru924Rw7qmJ7Asw";


            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));
            var a = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                ruta,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("Fotos_Perfil")
                .Child(nombre)
                .PutAsync(archivo, cancellation.Token);


            var downloadURL = await task;


            return downloadURL;


        }


        private async Task UpdateProductoImage(int idProducto, string nuevaUrl)
        {
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:cadenaSQL"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_update_producto_image", cn); // Probablemente un procedimiento almacenado

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idpro", idProducto);
                cmd.Parameters.AddWithValue("@foto", nuevaUrl);

                await cmd.ExecuteNonQueryAsync();
            }
        }

    }
}
