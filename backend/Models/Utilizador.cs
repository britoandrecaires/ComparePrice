using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaPrecos.API.Models
{
    [Table("utilizadores")]
    public class Utilizador
    {
        [Key]
        [Column("utilizador_id")]
        public int UtilizadorId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("tipo_utilizador_id")]
        public int TipoUtilizadorId { get; set; }

        [ForeignKey("TipoUtilizadorId")]
        public TipoUtilizador TipoUtilizador { get; set; }
         [Column("ativo")]
        public bool Ativo { get; set; } = true;
    }
}
