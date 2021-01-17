using System.ComponentModel.DataAnnotations.Schema;

namespace ConsejeriaEpics.Models
{
    [Table("t_categorias")]
    public class Categoria
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }
    }
}