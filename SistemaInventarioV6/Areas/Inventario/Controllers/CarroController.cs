using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos.ViewModels;
using SistemaInventarioV6.Utilidades;
using System.Security.Claims;

namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    public class CarroController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;

        [BindProperty]
        public CarroCompraVM carroCompraVM { get; set; }

        public CarroController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //capturar al usuario del carro de compra
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Obtener el carro de compra guardado en BD
            carroCompraVM = new CarroCompraVM();
            carroCompraVM.Orden = new Modelos.Orden();   //iniciar una orden vacia
            carroCompraVM.CarroCompraLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(
                u => u.UsuarioAplicacionId == claim.Value,
                incluirPropiedades:"Producto"
            );

            carroCompraVM.Orden.TotalOrden = 0;
            carroCompraVM.Orden.UsuarioAplicacionId = claim.Value;

            //recorremos el carro de compra y todos sus productos
            foreach (var lista in carroCompraVM.CarroCompraLista)
            {
                lista.Precio = lista.Producto.Precio;  //siempre mostrar el precio actual del producto
                carroCompraVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);
            }
            return View(carroCompraVM);
        }


        public async Task<IActionResult> mas(int carroId)
        {
            var carroCompras = await _unidadTrabajo.CarroCompra.ObtenerPrimero(c => c.Id == carroId);
            carroCompras.Cantidad += 1;
            await _unidadTrabajo.Guardar();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> menos(int carroId)
        {
            var carroCompras = await _unidadTrabajo.CarroCompra.ObtenerPrimero(c => c.Id == carroId);
            
            if(carroCompras.Cantidad == 1)
            {
                //quitando 1 queda en valor cero, el registro se remueve
                //Removemos el registro del carro de compras y actualizamos la sesion
                var carroLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(
                    c => c.UsuarioAplicacionId == carroCompras.UsuarioAplicacionId
                );

                var numeroProductos = carroLista.Count();
                _unidadTrabajo.CarroCompra.Remover(carroCompras);
                await _unidadTrabajo.Guardar();


                //actualizar la sesion
                HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos - 1);
            }
            else
            {
                carroCompras.Cantidad -= 1;
                await _unidadTrabajo.Guardar();

            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> remover (int carroId)
        {
            //Remueve el registro de carro de compras y actualiza la sesión
            var carroCompras = await _unidadTrabajo.CarroCompra.ObtenerPrimero(c => c.Id == carroId);

            var carroLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(
                    c => c.UsuarioAplicacionId == carroCompras.UsuarioAplicacionId
                );

            var numeroProductos = carroLista.Count();
            _unidadTrabajo.CarroCompra.Remover(carroCompras);
            await _unidadTrabajo.Guardar();

            //actualizar la sesion
            HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos - 1);

            return RedirectToAction("Index");
        }
    }
}
