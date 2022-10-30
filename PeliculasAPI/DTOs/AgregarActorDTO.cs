using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class AgregarActorDTO
    {
        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public IFormFile Foto { get; set; }
    }
}
