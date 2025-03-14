using SistemaInventarioV6.AccesoDatos.Data;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioV6.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.AccesoDatos.Repositorio
{
    public class CarroCompraRepositorio : Repositorio<CarroCompra>, ICarroCompraRepositorio
    {
        private readonly ApplicationDbContext _db;

        public CarroCompraRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Actualizar(CarroCompra carroCompra)
        {
            _db.Update(carroCompra);  //se actualiza todo el registro de una vez
        }
    }
}
