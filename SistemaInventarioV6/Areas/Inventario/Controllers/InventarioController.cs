
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.Modelos.ViewModels;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Modelos.ViewModels;
using SistemaInventarioV6.Utilidades;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Rotativa.AspNetCore;
using System.Globalization;

namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    [Authorize(Roles = DS.Role_Admin + "," + DS.Role_Inventario)]
    public class InventarioController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;

        [BindProperty]
        public InventarioVM inventarioVM { get; set; }

        public InventarioController (IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NuevoInventario()
        {
            inventarioVM = new InventarioVM()
            {
                Inventario = new Modelos.Inventario(),
                BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega")
            };

            inventarioVM.Inventario.Estado = false;

            //obteniendo el id del usuario desde la sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            inventarioVM.Inventario.UsuarioAplicacionId = claim.Value;
            inventarioVM.Inventario.FechaInicial = DateTime.Now;
            inventarioVM.Inventario.FechaFinal = DateTime.Now;

            return View(inventarioVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoInventario(InventarioVM inventarioVM)
        {
            if(ModelState.IsValid)
            {
                inventarioVM.Inventario.FechaInicial = DateTime.Now;
                inventarioVM.Inventario.FechaFinal = DateTime.Now;
                await _unidadTrabajo.Inventario.Agregar(inventarioVM.Inventario);
                await _unidadTrabajo.Guardar();

                //redireccionamos a la vista de ingreso de productos (y pasamos el id inventario creado)
                return RedirectToAction("DetalleInventario" , new { id = inventarioVM.Inventario.Id});
            }

            //cuando no este válido, se llena la lista de bodegas y se returna a la vista actual
            inventarioVM.BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega");
            return View(inventarioVM);
        }

        public async Task<IActionResult> DetalleInventario ( int id)
        {
            inventarioVM = new InventarioVM();  //crear inventario nuevo vacio

            //obtener inventario segun el id
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.ObtenerPrimero(i => i.Id == id, incluirPropiedades: "Bodega");
            inventarioVM.InventarioDetalles = await _unidadTrabajo.InventarioDetalle.ObtenerTodos(
                d => d.InventarioId == id,
                incluirPropiedades: "Producto,Producto.Marca"
            );

            return View(inventarioVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetalleInventario(int InventarioId , int productoId, int cantidadId)
        {
            inventarioVM = new InventarioVM();  //inicializar inventarioVM
            //obtener el inventario con el que se va a trabajar
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.ObtenerPrimero(i => i.Id == InventarioId);

            //verificar si ese producto ya existe y esta creado con stock en la tabla BodegaProducto
            var bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(
                    b => b.ProductoId == productoId && b.BodegaId == inventarioVM.Inventario.BodegaId );

            //validando si hay un detalle de producto existente (si existe, solo se sumaran las cantidades del inventario que se esta ingresando)
            var detalle = await _unidadTrabajo.InventarioDetalle.ObtenerPrimero(
                d => d.InventarioId == InventarioId && d.ProductoId == productoId);

            if(detalle == null)
            {
                //si entra aca, no hay un detallde inventario existente para la combinacion idProducto - idBodega
                // - Se inicializa y crea un InventarioDetalle nuevo con productoId para el inventarioId
                // - Se verifica si hay inventario en bodegas para BodegaId - ProductoId
                //   -- si existe, se toma el stock actual y ese valor será el stockAnterior en el nuevo inventario a ingresar
                //   -- si no existe, el valor de stockAnterior será 0 (cero)
                // - se coloca la cantidad en el detalle de inventario
                // - guardar datos


                //inicializando InventarioDetalle
                inventarioVM.InventarioDetalle = new InventarioDetalle();
                inventarioVM.InventarioDetalle.ProductoId = productoId;
                inventarioVM.InventarioDetalle.InventarioId = InventarioId;

                //existe un registro en tabla BodegaProducto
                if(bodegaProducto != null)
                {
                    //se guarda la cantidad existente en inventario en el stockAnterior
                    inventarioVM.InventarioDetalle.StockAnterior = bodegaProducto.Cantidad;
                }
                else
                {
                    //no hay registro en la tabla BodegaProducto
                    //stockAnterior = 0
                    inventarioVM.InventarioDetalle.StockAnterior = 0;
                }

                //el nuevo inventario a ingresar se coloca en cantidad
                inventarioVM.InventarioDetalle.Cantidad = cantidadId;

                //guardar datos
                await _unidadTrabajo.InventarioDetalle.Agregar(inventarioVM.InventarioDetalle);
                await _unidadTrabajo.Guardar();
            }
            else 
            {
                //si entra aca, hay un detalle inventario existente, solo se sumaran las cantidades
                detalle.Cantidad += cantidadId;
                await _unidadTrabajo.Guardar();
            }

            //redireccionando a vista DetalleInventario
            return RedirectToAction("DetalleInventario", new { id = InventarioId });
        }

        //metodos para aumentar y disminuir las cantidades de stock
        public async Task<IActionResult> Mas(int id)   // recibe el id del detalle
        {
            inventarioVM = new InventarioVM();  //inicializar inventario
            var detalle = await _unidadTrabajo.InventarioDetalle.Obtener(id);  //obtener el detalle de inventario
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.Obtener(detalle.InventarioId);  //obtener el inventario

            detalle.Cantidad += 1;
            await _unidadTrabajo.Guardar();
            return RedirectToAction("DetalleInventario" , new { id = inventarioVM.Inventario.Id });
        }

        public async Task<IActionResult> Menos(int id)   // recibe el id del detalle
        {
            inventarioVM = new InventarioVM();  //inicializar inventario
            var detalle = await _unidadTrabajo.InventarioDetalle.Obtener(id);  //obtener el detalle de inventario
            inventarioVM.Inventario = await _unidadTrabajo.Inventario.Obtener(detalle.InventarioId);  //obtener el inventario

            if (detalle.Cantidad == 1)  //cuando cantidad sea 1
            {
                //eliminar el detalle de inventario, ya que no puede ser cero
                _unidadTrabajo.InventarioDetalle.Remover(detalle);
                await _unidadTrabajo.Guardar();
            }
            else
            {
                detalle.Cantidad -= 1;
                await _unidadTrabajo.Guardar();
            }
            return RedirectToAction("DetalleInventario", new { id = inventarioVM.Inventario.Id });
        }

        //metodo generarStock
        public async Task<IActionResult> GenerarStock( int id)
        {
            var inventario = await _unidadTrabajo.Inventario.Obtener(id);  //inventario que se va a ingresar a stock
            var detalleLista = await _unidadTrabajo.InventarioDetalle.ObtenerTodos(d => d.InventarioId == id);  //detalle del inventario a ingresar

            //obteniendo el id del usuario desde la sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            foreach (var item in detalleLista)
            {
                var bodegaProducto = new BodegaProducto();

                //verificando que el registro para bodega-producto existe en la BD
                bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(
                    b => b.ProductoId == item.ProductoId &&
                    b.BodegaId == inventario.BodegaId);

                if (bodegaProducto != null)  // el registro de stock existe, hay que actualizar las cantidades
                {
                    //registrar kardex antes de actualizar la cantidad
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(
                        bodegaProducto.Id , 
                        "Entrada" , 
                        "Registro de Inventario" ,
                        bodegaProducto.Cantidad,
                        item.Cantidad,
                        claim.Value
                        );
                    bodegaProducto.Cantidad += item.Cantidad;   //sumamos la cantidad al stock
                    await _unidadTrabajo.Guardar();
                }
                else  //registro de stock no existe, hay que crearlo
                {
                    bodegaProducto = new BodegaProducto();
                    bodegaProducto.BodegaId = inventario.BodegaId;
                    bodegaProducto.ProductoId = item.ProductoId;
                    bodegaProducto.Cantidad = item.Cantidad;
                    await _unidadTrabajo.BodegaProducto.Agregar(bodegaProducto);
                    await _unidadTrabajo.Guardar();

                    //registrar kardex antes de actualizar la cantidad
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(
                        bodegaProducto.Id,
                        "Entrada",
                        "Inventario Inicial",
                        0,  //no hay stock anterior, por tanto 0 (cero)
                        item.Cantidad,
                        claim.Value
                     );
                }

            }

            //Actualizar la cabecera del inventario
            inventario.Estado = true;
            inventario.FechaFinal = DateTime.Now;
            await _unidadTrabajo.Guardar();
            TempData[DS.Exitosa] = "Stock Generado con Exito";
            return RedirectToAction("Index");
        }


        public IActionResult KardexProducto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KardexProducto(string fechaInicioId, string fechaFinalId , int productoId)
        {
            return RedirectToAction("KardexProductoResultado" , new { fechaInicioId , fechaFinalId , productoId });
        }

        public async Task<IActionResult> KardexProductoResultado(string fechaInicioId, string fechaFinalId, int productoId)
        {
            KardexInventarioVM kardexInventarioVM = new KardexInventarioVM();
            kardexInventarioVM.Producto = new Producto();
            kardexInventarioVM.Producto = await _unidadTrabajo.Producto.Obtener(productoId);

            kardexInventarioVM.FechaInicio = DateTime.Parse(fechaInicioId); //  00:00:00
            kardexInventarioVM.FechaFinal = DateTime.Parse(fechaFinalId).AddHours(23).AddMinutes(59);

            kardexInventarioVM.KardexInventarioLista = await _unidadTrabajo.KardexInventario.ObtenerTodos(
                                                                   k => k.BodegaProducto.ProductoId == productoId &&
                                                                       (k.FechaRegistro >= kardexInventarioVM.FechaInicio &&
                                                                        k.FechaRegistro <= kardexInventarioVM.FechaFinal),
                                            incluirPropiedades: "BodegaProducto,BodegaProducto.Producto,BodegaProducto.Bodega",
                                            orderBy: o => o.OrderBy(o => o.FechaRegistro)
                );

            return View(kardexInventarioVM);
        }


        public async Task<IActionResult> ImprimirKardex(string fechaInicio, string fechaFinal, int productoId)
        {
            KardexInventarioVM kardexInventarioVM = new KardexInventarioVM();
            kardexInventarioVM.Producto = new Producto();
            kardexInventarioVM.Producto = await _unidadTrabajo.Producto.Obtener(productoId);


            kardexInventarioVM.FechaInicio = DateTime.Parse(fechaInicio);
            kardexInventarioVM.FechaFinal = DateTime.Parse(fechaFinal);

            kardexInventarioVM.KardexInventarioLista = await _unidadTrabajo.KardexInventario.ObtenerTodos(
                                                                   k => k.BodegaProducto.ProductoId == productoId &&
                                                                       (k.FechaRegistro >= kardexInventarioVM.FechaInicio &&
                                                                        k.FechaRegistro <= kardexInventarioVM.FechaFinal),
                                            incluirPropiedades: "BodegaProducto,BodegaProducto.Producto,BodegaProducto.Bodega",
                                            orderBy: o => o.OrderBy(o => o.FechaRegistro)
                );

            return new ViewAsPdf("ImprimirKardex", kardexInventarioVM)
            {
                FileName = "KardexProducto.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                CustomSwitches = "--page-offset 0 --footer-center [page] --footer-font-size 12"
            };
        }

        #region API
        [HttpGet]

        //Método para devolver todo el inventario
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.BodegaProducto.ObtenerTodos(incluirPropiedades: "Bodega,Producto");
            return Json(new {data = todos});
        }

        [HttpGet]
        //busqueda de producto con el termino de busqueda, mediante la librería de tercero select2
        public async Task<IActionResult> BuscarProducto(string term)
        {
            //verificar que el termino de busqueda no sea nulo
            if(!string.IsNullOrEmpty(term))
            {
                //consultar lista de productos activos
                var listaProductos = await _unidadTrabajo.Producto.ObtenerTodos(p => p.Estado == true);

                //obteermos la lista de productos filtrando el número de serie o la descripción coincidentes con term
                var data = listaProductos.Where(
                    x => x.NumeroSerie.Contains(
                        term, StringComparison.OrdinalIgnoreCase) ||
                        x.Descripcion.Contains(term, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                return Ok(data);
            }
            return Ok();
        }
        #endregion
    }
}
