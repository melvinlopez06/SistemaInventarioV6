using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.Modelos.Especificaciones
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; set; }

        public PagedList( List<T> items , int count , int pageNumber , int pageSize)
        {
            MetaData = new MetaData()
            {
                TotalCount = count,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)   //por ejemplo 1.5 lo transforma a 2
            };
            AddRange(items);   //agrega los elementos de la colección al final de la lista
        }

        public static PagedList<T> ToPagedList(IEnumerable<T> entidad, int pageNumber, int pageSize)
        {
            var count = entidad.Count();
            var items = entidad.Skip((pageNumber - 1)  * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items , count , pageNumber , pageSize);
        }
    }
}
