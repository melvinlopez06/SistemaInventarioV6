using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.Modelos
{
    public class Marca
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage="Nombre es Requerido")]
        [MaxLength(60, ErrorMessage ="Nombre debe ser máximo 60 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage ="Descripción es Requerido")]
        [MaxLength(100, ErrorMessage="Descripción debe ser máximo 100 caracteres")]
        public string Descripcion { get; set; }


        [Required(ErrorMessage = "Estado es Requerido")]
        public bool Estado { get; set; }

    }
}
