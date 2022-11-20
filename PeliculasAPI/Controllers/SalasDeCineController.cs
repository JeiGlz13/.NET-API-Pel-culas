using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [Route("api/SalasDeCine")]
    [ApiController]
    public class SalasDeCineController: CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;
        private readonly ApplicationDbContext _context;
        public SalasDeCineController(
            ApplicationDbContext context,
            IMapper mapper,
            GeometryFactory geometryFactory
        ):base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _geometryFactory = geometryFactory;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalaDeCineDTO>>> Get()
        {
            return await Get<SalaDeCine, SalaDeCineDTO>();
        }


        [HttpGet("{id:int}", Name = "obtenerSalaDeCine")]
        public async Task<ActionResult<SalaDeCineDTO>> Get(int id)
        {
            return await Get<SalaDeCine, SalaDeCineDTO>(id);
        }

        [HttpGet("Cercanos")]
        public async Task<ActionResult<List<SalaDeCineCercanoDTO>>> Cercanos(
            [FromQuery] SalaDeCineCercanoFiltroDTO filtro
        )
        {
            var ubicacionUsuario = _geometryFactory.CreatePoint(new Coordinate(filtro.Longitud, filtro.Latitud));
            var salasDeCine = await _context.SalasDeCine
                .OrderBy(x => x.Ubicacion.Distance(ubicacionUsuario))
                .Where(x => x.Ubicacion.IsWithinDistance(ubicacionUsuario, filtro.DistanciaEnKM * 1000))
                .Select(x => new SalaDeCineCercanoDTO
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Latitud = x.Ubicacion.Y,
                    Longitud = x.Ubicacion.X,
                    DistanciaEnMetros = Math.Round(x.Ubicacion.Distance(ubicacionUsuario)),
                })
                .ToListAsync();

            return salasDeCine;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AgregarSalaDeCineDTO agregarSalaDeCineDTO)
        {
            return await Post<AgregarSalaDeCineDTO, SalaDeCine, SalaDeCineDTO>(agregarSalaDeCineDTO, "obtenerSalaDeCine");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] AgregarSalaDeCineDTO agregarSalaDeCineDTO)
        {
            return await Put<AgregarSalaDeCineDTO, SalaDeCine>(id, agregarSalaDeCineDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<SalaDeCine>(id);
        }
    }
}
