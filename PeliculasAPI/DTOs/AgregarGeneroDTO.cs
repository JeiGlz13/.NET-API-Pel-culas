using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class AgregarGeneroDTO
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}
