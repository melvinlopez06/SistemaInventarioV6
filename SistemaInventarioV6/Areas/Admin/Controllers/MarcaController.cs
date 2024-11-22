using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Utilidades;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    //especificar el area de trabajo para que el routing funcione
    [Area("Admin")]
    public class MarcaController : Controller
    {
        //referenciar a la unidad de trabajo
        private readonly IUnidadTrabajo _unidadTrabajo;

        public MarcaController( IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert (int? id)  //? porque puede ser nulo
        {
            Marca marca = new Marca();

            if(id == null)
            {
                //crear una nueva marca con estado activo
                marca.Estado = true;
                return View(marca);
            } else
            {
                //Actualizamos una marca existente
                marca = await _unidadTrabajo.Marca.Obtener(id.GetValueOrDefault());
                if(marca == null)
                {
                    return NotFound();
                }
                return View(marca);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Marca marca)
        {
            if (ModelState.IsValid)  //valida que todas las propiedades del modelo sean válidas
            {
                if (marca.Id == 0)
                {
                    //Nuevo registro
                    await _unidadTrabajo.Marca.Agregar(marca);
                    TempData[DS.Exitosa] = "Marca creada Exitosamente";
                }
                else
                {
                    _unidadTrabajo.Marca.Actualizar(marca);
                    TempData[DS.Exitosa] = "Marca actualizada Exitosamente";
                }
                await _unidadTrabajo.Guardar();

                //luego de guardar se redirecciona
                return RedirectToAction(nameof(Index));
            }
            TempData[DS.Error] = "Error al grabar Marca";

            //si no pasa la validación del modelo, se envía a la vista de Marca
            return View(marca);
        }

        #region API

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Marca.ObtenerTodos();
            return Json( new {data = todos});
            //data es el nombre con que se va a referenciar la respuesta en javascript
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var marcaDb = await _unidadTrabajo.Marca.Obtener(id);
            if(marcaDb == null)
            {
                return Json(new { success= false, message = "Error al borrar Marca" });
            }
            _unidadTrabajo.Marca.Remover(marcaDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Marca borrada exitosamente" });

        }

        [ActionName("ValidarNombre")]
        public async Task<IActionResult> ValidarNombre(string nombre , int id=0 )
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Marca.ObtenerTodos();
            if(id == 0)
            {
                //buscando marcas con el mismo nombre para nuevos registros
                valor = lista.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            } else
            {
                //buscando marcas con el mismo nombre para registros existentes
                valor = lista.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim() && b.Id != id);
            }

            if(valor)
            {
                //encontro una coincidencia con una marca del mismo nombre
                return Json(new { data = true });
            }
            //no encontro coincidencias con el mismo nombre de marca
            return Json(new { data = false });
        }

        #endregion
    }
}
