using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Utilidades;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    //especificar el area de trabajo para que el routing funcione
    [Area("Admin")]
    [Authorize(Roles = DS.Role_Admin)]   //obligar a que el usuario se autentique y que sea admin
    public class BodegaController : Controller
    {
        //referenciar a la unidad de trabajo
        private readonly IUnidadTrabajo _unidadTrabajo;

        public BodegaController( IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert (int? id)  //? porque puede ser nulo
        {
            Bodega bodega = new Bodega();

            if(id == null)
            {
                //crear una nueva bodega con estado activo
                bodega.Estado = true;
                return View(bodega);
            } else
            {
                //Actualizamos una bodega existente
                bodega = await _unidadTrabajo.Bodega.Obtener(id.GetValueOrDefault());
                if(bodega == null)
                {
                    return NotFound();
                }
                return View(bodega);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Bodega bodega)
        {
            if (ModelState.IsValid)  //valida que todas las propiedades del modelo sean válidas
            {
                if (bodega.Id == 0)
                {
                    //Nuevo registro
                    await _unidadTrabajo.Bodega.Agregar(bodega);
                    TempData[DS.Exitosa] = "Bodega creada Exitosamente";
                }
                else
                {
                    _unidadTrabajo.Bodega.Actualizar(bodega);
                    TempData[DS.Exitosa] = "Bodega actualizada Exitosamente";
                }
                await _unidadTrabajo.Guardar();

                //luego de guardar se redirecciona
                return RedirectToAction(nameof(Index));
            }
            TempData[DS.Error] = "Error al grabar Bodega";

            //si no pasa la validación del modelo, se envía a la vista de Bodega
            return View(bodega);
        }

        #region API

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Bodega.ObtenerTodos();
            return Json( new {data = todos});
            //data es el nombre con que se va a referenciar la respuesta en javascript
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var bodegaDb = await _unidadTrabajo.Bodega.Obtener(id);
            if(bodegaDb == null)
            {
                return Json(new { success= false, message = "Error al borrar Bodega" });
            }
            _unidadTrabajo.Bodega.Remover(bodegaDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Bodega borrada exitosamente" });

        }

        [ActionName("ValidarNombre")]
        public async Task<IActionResult> ValidarNombre(string nombre , int id=0 )
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Bodega.ObtenerTodos();
            if(id == 0)
            {
                //buscando bodegas con el mismo nombre para nuevos registros
                valor = lista.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            } else
            {
                //buscando bodegas con el mismo nombre para registros existentes
                valor = lista.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim() && b.Id != id);
            }

            if(valor)
            {
                //encontro una coincidencia con una bodega del mismo nombre
                return Json(new { data = true });
            }
            //no encontro coincidencias con el mismo nombre de bodega
            return Json(new { data = false });
        }

        #endregion
    }
}
