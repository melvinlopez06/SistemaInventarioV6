using SistemaInventarioV6.AccesoDatos.Data;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.AccesoDatos.Repositorio
{
    public class UnidadTrabajo : IUnidadTrabajo
    {
        private readonly ApplicationDbContext _db;

        public IBodegaRepositorio Bodega { get; private set; }
        public ICategoriaRepositorio Categoria { get; private set; }

        public UnidadTrabajo (ApplicationDbContext db)
        {
            _db = db;
            Bodega = new BodegaRepositorio (_db);
            Categoria = new CategoriaRepositorio(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        async public Task Guardar()
        {
            await _db.SaveChangesAsync();
        }
    }
}
