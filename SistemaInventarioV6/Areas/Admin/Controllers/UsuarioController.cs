using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioV6.AccesoDatos.Data;
using SistemaInventarioV6.AccesoDatos.Repositorio.IRepositorio;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsuarioController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly ApplicationDbContext _db;

        public UsuarioController( IUnidadTrabajo unidadTrabajo , ApplicationDbContext db)
        {
            _unidadTrabajo = unidadTrabajo;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var usuarioLista = await _unidadTrabajo.UsuarioAplicacion.ObtenerTodos();
            var userRole = await _db.UserRoles.ToListAsync();
            var roles = await _db.Roles.ToListAsync();

            foreach (var usuario in usuarioLista)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == usuario.Id).RoleId;
                usuario.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            return Json(new { data = usuarioLista });
        }

        [HttpPost]
        public async Task<IActionResult> BloquearDesbloquear([FromBody] string id)
        {
            var usuario = await _unidadTrabajo.UsuarioAplicacion.ObtenerPrimero(u => u.Id == id);
            if(usuario == null)
            {
                return Json(new { success = false, message = "Error de usuario" });
            }

            //usuario bloqueado cuando lockOutEnd tiene una fecha mayor a la fecha en curso
            if (usuario.LockoutEnd != null && usuario.LockoutEnd > DateTime.Now)
            {
                //desbloqueamos usuario colocando now a lockOutEnd
                usuario.LockoutEnd = DateTime.Now;
            }
            else
            {
                //bloqueamos usuario agregando años o seteando una fecha posterior a LockoutEnd
                usuario.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Operación Exitosa" });
        }
        #endregion
    }
}
