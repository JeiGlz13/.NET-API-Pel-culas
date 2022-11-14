using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;
using PeliculasAPI.Servicios;
using System.Linq.Dynamic.Core;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly string _contenedor = "poster";
        private readonly ILogger<PeliculasController> _logger;
        public PeliculasController(
            ApplicationDbContext context,
            IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos,
            ILogger<PeliculasController> logger
        ): base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _almacenadorArchivos = almacenadorArchivos;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<PeliculaDTO>>> GetPeliculas()
        {
            return await Get<Pelicula, PeliculaDTO>();
        }

        [HttpGet("GetCartelera")]
        public async Task<ActionResult<PeliculasIndexDTO>> GetCartelera()
        {
            var top = 5;
            var hoy = DateTime.Today;

            var proximosEstrenos = await _context.Peliculas
                .Where(p => p.FechaEstreno > hoy)
                .OrderBy(p => p.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var enCines = await _context.Peliculas
                .Where(p => p.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDTO
            {
                FuturosEstrenos = _mapper.Map<List<PeliculaDTO>>(proximosEstrenos),
                EnCines = _mapper.Map<List<PeliculaDTO>>(enCines)
            };
            return resultado;
        }

        [HttpGet("Filtro")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] FiltroPeliculaDTO filtroPeliculasDTO)
        {
            var peliculasQueryable = _context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.Titulo))
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.Titulo.Contains(filtroPeliculasDTO.Titulo));
            }

            if (filtroPeliculasDTO.EnCines)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.EnCines);
            }

            if (filtroPeliculasDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.FechaEstreno > hoy);
            }

            if (filtroPeliculasDTO.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.PeliculasGeneros.Select(genero => genero.GeneroId)
                    .Contains(filtroPeliculasDTO.GeneroId));
            }

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.CampoOrdenar))
            {
                var tipoOrden = filtroPeliculasDTO.OrdenAscendente ? "ascending" : "descending";
                try
                {
                    peliculasQueryable.OrderBy($"{filtroPeliculasDTO.Titulo} ${tipoOrden}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }

            await HttpContext.InsertarParametrosPaginacion(
                peliculasQueryable,
                filtroPeliculasDTO.RegistrosPorPagina
            );

            var peliculas = await peliculasQueryable
                .Paginar(filtroPeliculasDTO.Paginacion)
                .ToListAsync();
            return _mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpGet("GetPelicula/{id}", Name = "obtenerPelicula")]
        public async Task<ActionResult<PeliculaDetallesDTO>> GetPelicula(int id)
        {
            var pelicula = await _context.Peliculas
                .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(pelicula => pelicula.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            pelicula.PeliculasActores = pelicula.PeliculasActores.OrderBy(x => x.Orden).ToList();

            return _mapper.Map<PeliculaDetallesDTO>(pelicula);
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
            AsignarOrdenActores(entidad);
            _context.Add(entidad);
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<PeliculaDTO>(entidad);
            return new CreatedAtRouteResult("obtenerPelicula", new { id = entidad.Id, dto });
        }

        private void AsignarOrdenActores(Pelicula pelicula)
        {
            if (pelicula.PeliculasActores != null)
            {
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutPelicula (int id, [FromForm] AgregarPeliculaDTO agregarPeliculaDTO)
        {
            var peliculaDb = await _context.Peliculas
                .Include(x => x.PeliculasActores)
                .Include(x => x.PeliculasGeneros)
                .FirstOrDefaultAsync(pelicula => pelicula.Id == id);

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
            AsignarOrdenActores(peliculaDb);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> PatchActor(int id, [FromBody] JsonPatchDocument<PeliculaPatchDTO> patchDocument)
        {
            return await Patch<Pelicula, PeliculaPatchDTO>(id, patchDocument);
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeletePelicula(int id)
        {
            return await Delete<Pelicula>(id);
        }
    }
}
