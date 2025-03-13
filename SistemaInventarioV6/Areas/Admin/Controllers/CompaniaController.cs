using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos.ViewModels;
using SistemaInventarioV6.Utilidades;
using System.Security.Claims;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = DS.Role_Admin)]
    public class CompaniaController : Controller
    {

        private readonly IUnidadTrabajo _unidadTrabajo;

        public CompaniaController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        //No tendra metodo index, solo upsert para ver el registro de compañia y actualizarlo de ser necesario
        public async Task<IActionResult> Upsert()
        {
            CompaniaVM companiaVM = new CompaniaVM()
            {
                Compania = new SistemaInventarioV6.Modelos.Compania(),
                BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega")   //se reusa obtener bodegas desde metodo en Inventario
            };

            companiaVM.Compania = await _unidadTrabajo.Compania.ObtenerPrimero();  //obteniendo la compañia

            if (companiaVM.Compania == null)   //si compania no existe (la primera vez)
            {
                companiaVM.Compania = new SistemaInventarioV6.Modelos.Compania();  //compania en blanco
            }

            return View(companiaVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CompaniaVM companiaVM)
        {

            if (ModelState.IsValid)
            {
                TempData[DS.Exitosa] = "Compania grabada Exitosamente";   //mensaje de exito

                //obtener el usuario que esta en sesion
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (companiaVM.Compania.Id == 0)  // Crear la Compania
                {
                    companiaVM.Compania.CreadoPorId = claim.Value;
                    companiaVM.Compania.ActualizadoPorId = claim.Value;
                    companiaVM.Compania.FechaCreacion = DateTime.Now;
                    companiaVM.Compania.FechaActualizacion = DateTime.Now;
                    await _unidadTrabajo.Compania.Agregar(companiaVM.Compania);
                }
                else  // Actualizar Compania
                {
                    companiaVM.Compania.ActualizadoPorId = claim.Value;
                    companiaVM.Compania.FechaActualizacion = DateTime.Now;
                    _unidadTrabajo.Compania.Actualizar(companiaVM.Compania);
                }
                await _unidadTrabajo.Guardar();
                return RedirectToAction("Index", "Home", new { area = "Inventario" });    //redireccionamos al home del inventario especificando el area
            }
            TempData[DS.Error] = "Error al Grabar Compania";
            return View(companiaVM);
        }

    }
}
