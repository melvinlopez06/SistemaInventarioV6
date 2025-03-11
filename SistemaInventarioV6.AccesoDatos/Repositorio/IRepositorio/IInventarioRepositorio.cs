using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaInventarioV6.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio
{
    public interface IInventarioRepositorio : IRepositorio<Inventario>
    {
        void Actualizar(Inventario inventario);

        IEnumerable<SelectListItem> ObtenerTodosDropdownLista(string obj);
    }
}
