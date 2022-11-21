using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;

namespace PeliculasAPI.Controllers
{
    public class CustomBaseController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CustomBaseController(
            ApplicationDbContext context,
            IMapper mapper
        )
        {
            _context = context;
            _mapper = mapper;
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>()
        where TEntidad : class
        {
            var entidades = await _context.Set<TEntidad>()
                .AsNoTracking()
                .ToListAsync();
            var dtos = _mapper.Map<List<TDTO>>(entidades);
            return dtos;
        }

        protected async Task<ActionResult<TDTO>> Get<TEntidad, TDTO>(int id)
        where TEntidad : class, IId
        {
            var entidad = await _context.Set<TEntidad>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
            {
                return NotFound();
            }

            return _mapper.Map<TDTO>(entidad);
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDTO paginacionDTO)
            where TEntidad: class
        {
            var queryable = _context
                .Set<TEntidad>().AsQueryable();

            return await Get<TEntidad, TDTO>(paginacionDTO, queryable);
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDTO paginacionDTO, IQueryable<TEntidad> queryable)
        where TEntidad : class
        {
            await HttpContext.InsertarParametrosPaginacion(queryable, paginacionDTO.RegistrosPorPagina);
            var entidades = await _context.Set<TEntidad>()
                .Paginar(paginacionDTO).ToListAsync();

            return _mapper.Map<List<TDTO>>(entidades);
        }

        protected async Task<ActionResult> Post<TCreacion, TEntidad, TLectura>(TCreacion creacionDTO, string nombreRuta)
            where TEntidad: class, IId
        {
            var entidad = _mapper.Map<TEntidad>(creacionDTO);
            _context.Add(entidad);
            await _context.SaveChangesAsync();

            var dtoLectura = _mapper.Map<TLectura>(entidad);

            return new CreatedAtRouteResult(nombreRuta, new { id = entidad.Id }, dtoLectura);
        }

        protected async Task<ActionResult> Put<TCreacion, TEntidad>(int id, TCreacion creacionDTO)
        where TEntidad: class, IId
        {
            var entidad = _mapper.Map<TEntidad>(creacionDTO);
            entidad.Id = id;
            _context.Entry(entidad).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        protected async Task<ActionResult> Delete<TEntidad>(int id)
        where TEntidad: class, IId, new()
        {
            var existe = await _context.Set<TEntidad>()
                .AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new TEntidad() { Id = id});
            await _context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Patch<TEntidad, TDTO>(int id, JsonPatchDocument<TDTO> patchDocument)
        where TDTO: class
        where TEntidad : class, IId
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entidadDb = await _context.Set<TEntidad>().FirstOrDefaultAsync(x => x.Id == id);
            if (entidadDb == null) { return NotFound(); }

            var entidadDTO = _mapper.Map<TDTO>(entidadDb);
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
    }
}
