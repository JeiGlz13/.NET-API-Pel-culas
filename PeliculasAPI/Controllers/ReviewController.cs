using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;
using System.Security.Claims;

namespace PeliculasAPI.Controllers
{
    [Route("api/peliculas/{peliculaId:int}/review")]
    [ServiceFilter(typeof(PeliculaExisteAttribute))]
    public class ReviewController: CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public ReviewController(
            ApplicationDbContext context,
            IMapper mapper

        ): base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReviewDTO>>> Get(int peliculaId, [FromQuery] PaginacionDTO paginacionDTO)
        {
            var reviewUser = await _context.Reviews.Include(x => x.Usuario).ToListAsync();

            var queryable = _context.Reviews.Include(x => x.Usuario).AsQueryable();
            queryable = queryable.Where(x => x.PeliculaId == peliculaId);
            return await Get<Review, ReviewDTO>(paginacionDTO, queryable);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int peliculaId, [FromBody] AgregarReviewDTO agregarReviewDTO)
        {
            var usuarioId = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var reviewExiste = await _context.Reviews
                .AnyAsync(x => x.PeliculaId == peliculaId && x.UsuarioId == usuarioId);

            if (reviewExiste)
            {
                return BadRequest("El usuario ya ha escrito un review de la película");
            }

            var review = _mapper.Map<Review>(agregarReviewDTO);
            review.PeliculaId = peliculaId;
            review.UsuarioId = usuarioId;
            review.Usuario = await _context.Users.FirstOrDefaultAsync(x => x.Id == usuarioId);
            _context.Add(review);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int peliculaId, int reviewId, [FromBody] AgregarReviewDTO agregarReviewDTO)
        {
            var reviewDB = await _context.Reviews.FirstOrDefaultAsync(review => review.Id == reviewId);

            if (reviewDB == null)
            {
                return NotFound();
            }

            var usuarioId = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            if (reviewDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            reviewDB = _mapper.Map(agregarReviewDTO, reviewDB);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int reviewId)
        {
            var reviewDB = await _context.Reviews.FirstOrDefaultAsync(review => review.Id == reviewId);

            if (reviewDB == null)
            {
                return NotFound();
            }

            var usuarioId = HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            if (reviewDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            _context.Remove(reviewDB);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
