using PeliculasAPI.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class AgregarPeliculaDTO : PeliculaPatchDTO
    {
        [PesoArchivoValidacion(4)]
        [TipoArchivoValidacion(GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }
    }
}
