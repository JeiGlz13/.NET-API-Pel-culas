using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public ActoresController(
            ApplicationDbContext context,
            IMapper mapper
        )
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> GetActores()
        {
            var entidades = await _context.Actores.ToListAsync();
            return _mapper.Map<List<ActorDTO>>(entidades);
        }

        [HttpGet("{id:int}", Name = "ObtenerActor")]
        public async Task<ActionResult<ActorDTO>> GetActor(int id)
        {
            var entidad = await _context.Actores.FirstOrDefaultAsync(x => x.Id == id);
            if (entidad == null)
            {
                return NotFound();
            }

            return _mapper.Map<ActorDTO>(entidad);
        }

        [HttpPost]
        public async Task<ActionResult> PostActor([FromForm] AgregarActorDTO actorDTO)
        {
            var entidad = _mapper.Map<Actor>(actorDTO);
            _context.Actores.Add(entidad);
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<ActorDTO>(entidad);
            return new CreatedAtRouteResult("obtenerActor", new { id = entidad.Id, dto });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutActor (int id, [FromBody] AgregarActorDTO agregarActorDTO)
        {
            var entidad = _mapper.Map<Actor>(agregarActorDTO);
            entidad.Id = id;
            _context.Entry(entidad).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteActor(int idActor)
        {
            var existe = await _context.Actores
                .AnyAsync(actor => actor.Id == idActor);

            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Genero() { Id = idActor});
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
