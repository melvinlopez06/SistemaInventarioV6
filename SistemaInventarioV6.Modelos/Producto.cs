using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventarioV6.Modelos
{
    public class Producto
    {
        [Key]
        public int Id {  get; set; }

        

        [Required(ErrorMessage ="Número de Serie es Requerido")]
        [MaxLength(60)]
        public string NumeroSerie { get; set; }



        [Required(ErrorMessage ="Descripción es Requerido")]
        [MaxLength(60)]
        public string Descripcion { get; set; }



        [Required(ErrorMessage ="Precio es Requerido")]
        public double Precio { get; set; }



        [Required(ErrorMessage = "Costo es Requerido")]
        public double Costo { get; set; }



        public string ImagenUrl { get; set; }



        [Required(ErrorMessage = "Estado es Requerido")]
        public bool Estado { get; set; }



        [Required(ErrorMessage = "La categoria del producto es Requerida")]
        public int CategoriaId { get; set; }  //FK relacionando a tabla categorias

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }



        [Required(ErrorMessage = "La marca del producto es Requerida")]
        public int MarcaId {  get; set; }  //FK relacionando a la tabla marcas

        [ForeignKey("MarcaId")]
        public Marca Marca { get; set; }



        //PadreId servira para relacionar hacia la misma tabla de Producto y hacer recursividad
        public int? PadreId {  get; set; }  //? porque es necesario grabar como nulo en la BD cuando se crean registros

        public virtual Producto Padre { get; set; }

    }
}
