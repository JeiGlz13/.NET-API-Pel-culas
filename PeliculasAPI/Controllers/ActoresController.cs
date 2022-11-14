using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;
using PeliculasAPI.Helpers;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos _almacenadorArchivo;
        private readonly string _contenedor = "actores";
        public ActoresController(
            ApplicationDbContext context,
            IMapper mapper
,           IAlmacenadorArchivos almacenadorArchivo
        )
            :base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _almacenadorArchivo = almacenadorArchivo;

        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> GetActores([FromQuery] PaginacionDTO paginacionDTO)
        {
            return await Get<Actor, ActorDTO>(paginacionDTO);
        }

        [HttpGet("{id:int}", Name = "ObtenerActor")]
        public async Task<ActionResult<ActorDTO>> GetActor(int id)
        {
            return await Get<Actor, ActorDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> PostActor([FromForm] AgregarActorDTO actorDTO)
        {
            var entidad = _mapper.Map<Actor>(actorDTO);
            if (actorDTO.Foto != null)
            {
                using(var memoryStream = new MemoryStream())
                {
                    await actorDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorDTO.Foto.FileName);
                    entidad.Foto = await _almacenadorArchivo
                        .GuardarArchivo(contenido, extension, _contenedor, actorDTO.Foto.ContentType);
                }
            }
            _context.Add(entidad);
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<ActorDTO>(entidad);
            return new CreatedAtRouteResult("obtenerActor", new { id = entidad.Id, dto });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutActor (int id, [FromForm] AgregarActorDTO agregarActorDTO)
        {
            var actorDb = await _context.Actores.FirstOrDefaultAsync(actor => actor.Id == id);
            if (actorDb == null)
            {
                return NotFound();
            }

            actorDb = _mapper.Map(agregarActorDTO, actorDb);

            if (agregarActorDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await agregarActorDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(agregarActorDTO.Foto.FileName);
                    actorDb.Foto = await _almacenadorArchivo
                        .EditarArchivo(
                            contenido, extension, _contenedor,
                            actorDb.Foto, agregarActorDTO.Foto.ContentType
                         );
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> PatchActor (int id, [FromBody] JsonPatchDocument<ActorPatchDTO> patchDocument)
        {
            return await Patch<Actor, ActorPatchDTO>(id, patchDocument);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteActor(int id)
        {
            return await Delete<Actor>(id);
        }
    }
}
