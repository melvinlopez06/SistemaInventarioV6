using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Modelos.ViewModels;
using SistemaInventarioV6.Utilidades;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    //especificar el area de trabajo para que el routing funcione
    [Area("Admin")]
    [Authorize(Roles = DS.Role_Admin + "," + DS.Role_Inventario)]   //obligar a que el usuario se autentique y que sea admin o inventario
    public class ProductoController : Controller
    {
        //referenciar a la unidad de trabajo
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController( IUnidadTrabajo unidadTrabajo, IWebHostEnvironment webHostEnvironment)
        {
            _unidadTrabajo = unidadTrabajo;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert (int? id)  //? porque puede ser nulo
        {
            //instanciando producto con ViewModel para que la lista de categorias y marcas estén con el objeto producto
            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropDownLista("Categoria"),
                MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropDownLista("Marca"),
                PadreLista = _unidadTrabajo.Producto.ObtenerTodosDropDownLista("Producto")
            };

            if (id == null)
            {
                //Crear nuevo producto
                productoVM.Producto.Estado = true;  //activo por defecto, si viene inactivo desde formulario se guarda como inactivo
                return View(productoVM);
            } else
            {
                productoVM.Producto = await _unidadTrabajo.Producto.Obtener(id.GetValueOrDefault());
                if (productoVM.Producto == null)
                {
                    return NotFound();
                }
                return View(productoVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductoVM productoVM)
        {
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;  //captura de la imagen
                string webRootPath = _webHostEnvironment.WebRootPath;  //wwwroot
                if(productoVM.Producto.Id == 0)
                {
                    //crear un nuevo producto
                    //armamos la ruta, nombre y extensión del archivo subido
                    string upload = webRootPath + DS.ImagenRuta;  //ruta de la carpeta imagen en wwwroot
                    string filename = Guid.NewGuid().ToString();  //nombre aleatorio
                    string extension = Path.GetExtension(files[0].FileName);  //extensión del archivo subido en la posición 0 (si se sube más de un archivo hay más posiciones en el arreglo)

                    using( var fileStream = new FileStream(Path.Combine(upload, filename+extension) , FileMode.Create) )
                    {
                        files[0].CopyTo(fileStream);  //copiamos la imagen subida al directorio destino en wwwroot
                    }
                    productoVM.Producto.ImagenUrl = filename + extension;  //en la BD se guarda solo el nombre de imagen + extensión
                    await _unidadTrabajo.Producto.Agregar(productoVM.Producto); //se agrega el producto a la BD
                } else
                {
                    //actualizar el producto

                    //obtener el producto con el que se va a trabajar el update
                    var objProducto = await _unidadTrabajo.Producto.ObtenerPrimero( p => p.Id == productoVM.Producto.Id , isTracking:false);

                    if (files.Count > 0) //si se envía una nueva imagen con el formulario
                    {
                        //borrar la imagen anterior y reemplazar por la nueva imagen subida
                        string upload = webRootPath + DS.ImagenRuta;
                        string filename = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        //borrar la imagen anterior
                        var anteiorFile = Path.Combine(upload, objProducto.ImagenUrl);   //nombre de la imagen anterior
                        if(System.IO.File.Exists(anteiorFile))  //verificando que el archivo anterior exista
                        {
                            System.IO.File.Delete(anteiorFile);  //borrando la imagen anteior
                        }
                        using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);  //copiamos la imagen subida al directorio destino en wwwroot
                        }
                        productoVM.Producto.ImagenUrl = filename + extension;
                    } else
                    {
                        //no se envia una imagen nueva en el formulario, entonces se conserva el nombre existente en BD
                        productoVM.Producto.ImagenUrl = objProducto.ImagenUrl;
                    }
                    _unidadTrabajo.Producto.Actualizar(productoVM.Producto);
                }
                //notificacion de TempData
                TempData[DS.Exitosa] = "Transacción Exitosa";
                await _unidadTrabajo.Guardar();
                return View("Index");
            }

            //Si el modelo no es válido
            //rellenar de nuevo las listas de categoria y marca y devolver la vista
            productoVM.CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropDownLista("Categoria");
            productoVM.MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropDownLista("Marca");
            productoVM.PadreLista = _unidadTrabajo.Producto.ObtenerTodosDropDownLista("Producto");

            return View(productoVM);
        }

        #region API

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Producto.ObtenerTodos(incluirPropiedades:"Categoria,Marca");
            return Json( new {data = todos});
            //data es el nombre con que se va a referenciar la respuesta en javascript
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productoDb = await _unidadTrabajo.Producto.Obtener(id);
            if(productoDb == null)
            {
                return Json(new { success= false, message = "Error al borrar Producto" });
            }

            //borrar la imagen del producto
            string upload = _webHostEnvironment.WebRootPath + DS.ImagenRuta;   //ruta donde se almacenan las imagenes físicas
            var anteriorFile = Path.Combine(upload, productoDb.ImagenUrl);    //URL completa de imagen
            if(System.IO.File.Exists(anteriorFile))   //verificando si la imagen existe
            {
                System.IO.File.Delete(anteriorFile);   //borrando la imagen fisica
            }

            _unidadTrabajo.Producto.Remover(productoDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto borrado exitosamente" });

        }

        [ActionName("ValidarSerie")]
        public async Task<IActionResult> ValidarSerie(string serie , int id=0 )
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Producto.ObtenerTodos();
            if(id == 0)
            {
                //buscando productos con el mismo numero de serie para nuevos registros
                valor = lista.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim());
            } else
            {
                //buscando productos con el mismo numero de serie para registros existentes
                valor = lista.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim() && b.Id != id);
            }

            if(valor)
            {
                //encontro una coincidencia con una numero de serie del mismo nombre
                return Json(new { data = true });
            }
            //no encontro coincidencias con el mismo nombre de numero de serie
            return Json(new { data = false });
        }

        #endregion
    }
}
