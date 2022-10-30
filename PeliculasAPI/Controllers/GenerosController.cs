using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/Generos")]
    public class GenerosController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GenerosController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> GetGeneros()
        {
            var entidades = await _context.Generos.ToListAsync();
            var dtos = _mapper.Map<List<GeneroDTO>>(entidades);

            return dtos;
        }

        [HttpGet("{idGenero:int}", Name = "obtenerGenero")]
        public async Task<ActionResult<GeneroDTO>> GetGenero(int idGenero)
        {
            var entidad = await _context.Generos
                            .FirstOrDefaultAsync(genero => genero.Id == idGenero);

            if (entidad == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<GeneroDTO>(entidad);
            return dto;
        }

        [HttpPost]
        public async Task<ActionResult> PostGenero([FromBody] AgregarGeneroDTO agregarGeneroDTO)
        {
            var entidad = _mapper.Map<Genero>(agregarGeneroDTO);
            _context.Add(entidad);
            await _context.SaveChangesAsync();

            var generoDTO = _mapper.Map<GeneroDTO>(entidad);

            return new CreatedAtRouteResult("obtenerGenero", new {idGenero = generoDTO.Id}, generoDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] AgregarGeneroDTO agregarGeneroDTO)
        {
            var entidad = _mapper.Map<Genero>(agregarGeneroDTO);
            entidad.Id = id;
            _context.Entry(entidad).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteGenero(int idGenero)
        {
            var existe = await _context.Generos.AnyAsync(genero => genero.Id == idGenero);

            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Genero() { Id = idGenero});
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
