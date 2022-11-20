using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/Generos")]
    public class GenerosController: CustomBaseController
    {
        public GenerosController(ApplicationDbContext context, IMapper mapper)
            :base(context, mapper)
        {}

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> GetGeneros()
        {
            return await Get<Genero, GeneroDTO>();
        }

        [HttpGet("{id:int}", Name = "obtenerGenero")]
        public async Task<ActionResult<GeneroDTO>> GetGenero(int id)
        {
            return await Get<Genero, GeneroDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> PostGenero([FromBody] AgregarGeneroDTO agregarGeneroDTO)
        {
            return await Post<AgregarGeneroDTO, Genero, GeneroDTO>(agregarGeneroDTO, "obtenerGenero");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] AgregarGeneroDTO agregarGeneroDTO)
        {
            return await Put<AgregarGeneroDTO, Genero>(id, agregarGeneroDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteGenero(int idGenero)
        {
            return await Delete<Genero>(idGenero);
        }
    }
}
