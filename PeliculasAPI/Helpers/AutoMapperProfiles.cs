using AutoMapper;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<AgregarGeneroDTO, Genero>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<AgregarActorDTO, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore());

            CreateMap<ActorPatchDTO, Actor>().ReverseMap();
        }
    }
}
