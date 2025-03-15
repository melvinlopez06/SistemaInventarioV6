using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Modelos.Especificaciones;
using SistemaInventarioV6.Modelos.ViewModels;
using SistemaInventarioV6.Utilidades;
using System.Diagnostics;
using System.Security.Claims;

namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnidadTrabajo _unidadTrabajo;
        public CarroCompraVM carroCompraVM { get; set; }

        public HomeController(ILogger<HomeController> logger , IUnidadTrabajo unidadTrabajo)
        {
            _logger = logger;
            _unidadTrabajo = unidadTrabajo;
        }

        public async Task<IActionResult> Index(int pageNumber = 1 , string busqueda = "" , string busquedaActual= "")
        {
            //Controlar la sesión
                //obteniendo el usuario que esta en sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                //obtener la lista del carro de compra desde la BD para el usuario en sesion
                var carroLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(
                        c => c.UsuarioAplicacionId == claim.Value
                );
                var numeroProductos = carroLista.Count();  // Numero de Registros en el carro
                HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos);
            }

            //Validar que la busqueda no sea nula
            if ( !String.IsNullOrEmpty(busqueda))
            {
                pageNumber = 1;   //al buscar algo siempre se carga pagina 1
            } else
            {
                busqueda = busquedaActual;
            }
            ViewData["BusquedaActual"] = busqueda;

            //validar que numero de pagina siempre sea 1
            if (pageNumber < 1){ pageNumber = 1; }

            Parametros parametros = new Parametros()
            {
                PageNumber = pageNumber,
                PageSize = 10
            };

            var resultado = _unidadTrabajo.Producto.ObtenerTodosPaginado(parametros);

            if (!String.IsNullOrEmpty(busqueda))
            {
                //si existe un parametro de busqueda se pasa
                resultado = _unidadTrabajo.Producto.ObtenerTodosPaginado(parametros , p => p.Descripcion.Contains(busqueda));
            }

                ViewData["TotalPaginas"] = resultado.MetaData.TotalPages;
            ViewData["TotalRegistros"] = resultado.MetaData.TotalCount;
            ViewData["PageSize"] = resultado.MetaData.PageSize;
            ViewData["PageNumber"] = pageNumber;
            ViewData["Previo"] = "disabled";   //clase css bootstrap para desactivar el boton
            ViewData["Siguiente"] = "";

            //validando clase disabled del boton previo cuando pageNumber > 1
            if(pageNumber > 1) { ViewData["Previo"] = ""; }

            //si el total de paginas es <= que el número de pagina actual, se desactiva boton siguiente
            if(resultado.MetaData.TotalPages <= pageNumber ) { ViewData["Siguiente"] = "disabled"; }

            return View(resultado);
        }

        public async Task<IActionResult> Detalle (int id)
        {
            carroCompraVM = new CarroCompraVM();   //instanciar el VM
            carroCompraVM.Compania = await _unidadTrabajo.Compania.ObtenerPrimero();    //consultar la compañia
            carroCompraVM.Producto = await _unidadTrabajo.Producto.ObtenerPrimero(p => p.Id == id ,
                        incluirPropiedades: "Marca,Categoria");     //Obtener el producto y su respectiva marca y categoria asociados
            var bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(
                    b => b.ProductoId == id &&
                    b.BodegaId == carroCompraVM.Compania.BodegaVentaId);  //obtener el stock de la bodega de venta del producto respectivo

            if(bodegaProducto == null)  //no devuelve registro, significa stock 0
            {
                carroCompraVM.Stock = 0;
            }
            else
            {
                carroCompraVM.Stock = bodegaProducto.Cantidad;  //stock es la cantidad que hay en bodega
            }

            carroCompraVM.CarroCompra = new CarroCompra()   //se crea un carro compra con los datos encontrados de producto
            {
                Producto = carroCompraVM.Producto,
                ProductoId = carroCompraVM.Producto.Id
            };

            return View(carroCompraVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Detalle(CarroCompraVM carroCompraVM)
        {
            //obteniendo el usuario que esta en sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            carroCompraVM.CarroCompra.UsuarioAplicacionId = claim.Value;

            //validando si el usuario ya tiene un carroCompra en la BD
            CarroCompra carroBD = await _unidadTrabajo.CarroCompra.ObtenerPrimero(
                    c => c.UsuarioAplicacionId == claim.Value &&
                    c.ProductoId == carroCompraVM.CarroCompra.ProductoId);

            if (carroBD == null)  // no encontro un registro para ese usuario y producto en la tabla CarroCompra
            {
                await _unidadTrabajo.CarroCompra.Agregar(carroCompraVM.CarroCompra);
            }
            else
            {
                //hay un registro en la BD del carroCompra, solo se actualiza aumentando las cantidades
                carroBD.Cantidad += carroCompraVM.CarroCompra.Cantidad;
                _unidadTrabajo.CarroCompra.Actualizar(carroBD);
            }
            await _unidadTrabajo.Guardar();
            TempData[DS.Exitosa] = "Producto agregado al Carro de Compras";  //mensaje de exito

            // Agregar valor a la Sesion
            //obtener la lista del carro de compra desde la BD para el usuario en sesion
            var carroLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(
                    c => c.UsuarioAplicacionId == claim.Value
            );
            var numeroProductos = carroLista.Count();  // Numero de Registros en el carro
            HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos);

            return RedirectToAction("Index");

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
