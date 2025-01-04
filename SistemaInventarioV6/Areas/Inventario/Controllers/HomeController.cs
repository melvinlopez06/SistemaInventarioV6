using Microsoft.AspNetCore.Mvc;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using SistemaInventarioV6.Modelos.Especificaciones;
using SistemaInventarioV6.Modelos.ViewModels;
using System.Diagnostics;

namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnidadTrabajo _unidadTrabajo;

        public HomeController(ILogger<HomeController> logger , IUnidadTrabajo unidadTrabajo)
        {
            _logger = logger;
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index(int pageNumber = 1 , string busqueda = "" , string busquedaActual= "")
        {
            //Validar que la busqueda no sea nula
            if( !String.IsNullOrEmpty(busqueda))
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
                PageSize = 4
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
