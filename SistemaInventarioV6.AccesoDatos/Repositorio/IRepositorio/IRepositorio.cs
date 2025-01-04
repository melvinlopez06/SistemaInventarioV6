using SistemaInventarioV6.Modelos.Especificaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio
{
    public interface IRepositorio<T> where T : class
    {
        //encerrados en <TASK> para trabajarlos como metodos asincronos

        Task <T> Obtener(int id);

        Task <IEnumerable<T>> ObtenerTodos(
            Expression<Func<T, bool>> filtro = null,
            Func<IQueryable<T> , IOrderedQueryable<T>> orderBy =null,
            string incluirPropiedades = null,
            bool isTracking = true
            );

        PagedList<T> ObtenerTodosPaginado( Parametros parametros , Expression<Func<T, bool>> filtro = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string incluirPropiedades = null,
            bool isTracking = true
            );

        Task <T> ObtenerPrimero(
            Expression<Func<T, bool>> filtro = null,
            string incluirPropiedades = null,
            bool isTracking = true
            );

        Task Agregar(T entidad);


        //metodos de eliminar no pueden ser asincronos
        void Remover(T entidad);

        void RemoverRango(IEnumerable<T> entidad);

    }
}
