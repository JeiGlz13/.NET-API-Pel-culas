using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly string _contenedor = "poster";
        public PeliculasController(
            ApplicationDbContext context,
            IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos
        )
        {
            _context = context;
            _mapper = mapper;
            _almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<List<PeliculaDTO>>> GetPeliculas()
        {
            var peliculas = await _context.Peliculas.ToListAsync();
            return _mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpGet("{id}", Name = "obtenerPelicula")]
        public async Task<ActionResult<PeliculaDTO>> GetPelicula(int id)
        {
            var pelicula = await _context.Peliculas.FirstOrDefaultAsync(pelicula => pelicula.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            return _mapper.Map<PeliculaDTO>(pelicula);
        }

        [HttpPost]
        public async Task<ActionResult> PostPelicula([FromForm] AgregarPeliculaDTO agregarPeliculaDTO)
        {
            var entidad = _mapper.Map<Pelicula>(agregarPeliculaDTO);

            if (agregarPeliculaDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await agregarPeliculaDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(agregarPeliculaDTO.Poster.FileName);
                    entidad.Poster = await _almacenadorArchivos
                        .GuardarArchivo(contenido, extension, _contenedor, agregarPeliculaDTO.Poster.ContentType);
                }
            }
            _context.Add(entidad);
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<PeliculaDTO>(entidad);
            return new CreatedAtRouteResult("obtenerPelicula", new { id = entidad.Id, dto });
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutPelicula (int id, [FromForm] AgregarPeliculaDTO agregarPeliculaDTO)
        {
            var peliculaDb = await _context.Peliculas.FirstOrDefaultAsync(pelicula => pelicula.Id == id);
            if (peliculaDb == null)
            {
                return NotFound();
            }

            peliculaDb = _mapper.Map(agregarPeliculaDTO, peliculaDb);

            if (agregarPeliculaDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await agregarPeliculaDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(agregarPeliculaDTO.Poster.FileName);
                    peliculaDb.Poster = await _almacenadorArchivos
                        .EditarArchivo(
                            contenido, extension, _contenedor,
                            peliculaDb.Poster, agregarPeliculaDTO.Poster.ContentType
                         );
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> PatchActor(int id, [FromBody] JsonPatchDocument<PeliculaPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entidadDb = await _context.Peliculas.FirstOrDefaultAsync(pelicula => pelicula.Id == id);
            if (entidadDb == null) { return NotFound(); }

            var entidadDTO = _mapper.Map<PeliculaPatchDTO>(entidadDb);
            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);

            if (!esValido)
            {
                return BadRequest();
            }

            _mapper.Map(entidadDTO, entidadDb);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeletePelicula(int id)
        {
            var existe = await _context.Peliculas
                .AnyAsync(pelicula => pelicula.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Pelicula() { Id = id });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
