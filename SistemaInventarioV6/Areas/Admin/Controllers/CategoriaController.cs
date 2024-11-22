using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Utilidades;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    //especificar el area de trabajo para que el routing funcione
    [Area("Admin")]
    public class CategoriaController : Controller
    {
        //referenciar a la unidad de trabajo
        private readonly IUnidadTrabajo _unidadTrabajo;

        public CategoriaController( IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert (int? id)  //? porque puede ser nulo
        {
            Categoria categoria = new Categoria();

            if(id == null)
            {
                //crear una nueva categoria con estado activo
                categoria.Estado = true;
                return View(categoria);
            } else
            {
                //Actualizamos una categoria existente
                categoria = await _unidadTrabajo.Categoria.Obtener(id.GetValueOrDefault());
                if(categoria == null)
                {
                    return NotFound();
                }
                return View(categoria);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Categoria categoria)
        {
            if (ModelState.IsValid)  //valida que todas las propiedades del modelo sean válidas
            {
                if (categoria.Id == 0)
                {
                    //Nuevo registro
                    await _unidadTrabajo.Categoria.Agregar(categoria);
                    TempData[DS.Exitosa] = "Categoría creada Exitosamente";
                }
                else
                {
                    _unidadTrabajo.Categoria.Actualizar(categoria);
                    TempData[DS.Exitosa] = "Categoría actualizada Exitosamente";
                }
                await _unidadTrabajo.Guardar();

                //luego de guardar se redirecciona
                return RedirectToAction(nameof(Index));
            }
            TempData[DS.Error] = "Error al grabar Categoría";

            //si no pasa la validación del modelo, se envía a la vista de Categoria
            return View(categoria);
        }

        #region API

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Categoria.ObtenerTodos();
            return Json( new {data = todos});
            //data es el nombre con que se va a referenciar la respuesta en javascript
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var categoriaDb = await _unidadTrabajo.Categoria.Obtener(id);
            if(categoriaDb == null)
            {
                return Json(new { success= false, message = "Error al borrar Categoria" });
            }
            _unidadTrabajo.Categoria.Remover(categoriaDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Categoria borrada exitosamente" });

        }

        [ActionName("ValidarNombre")]
        public async Task<IActionResult> ValidarNombre(string nombre , int id=0 )
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Categoria.ObtenerTodos();
            if(id == 0)
            {
                //buscando categorias con el mismo nombre para nuevos registros
                valor = lista.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            } else
            {
                //buscando categorias con el mismo nombre para registros existentes
                valor = lista.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim() && b.Id != id);
            }

            if(valor)
            {
                //encontro una coincidencia con una categoria del mismo nombre
                return Json(new { data = true });
            }
            //no encontro coincidencias con el mismo nombre de categoria
            return Json(new { data = false });
        }

        #endregion
    }
}
