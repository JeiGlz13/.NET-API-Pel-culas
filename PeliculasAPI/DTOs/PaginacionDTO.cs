namespace PeliculasAPI.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;
        private int _registrosPorPagina = 10;
        private readonly int _maximoRegistroPorPagina = 50;

        public int RegistrosPorPagina
        {
            get { return _registrosPorPagina; }
            set
            {
                _registrosPorPagina = (value > _maximoRegistroPorPagina) ? _maximoRegistroPorPagina : value;
            }
        }
    }
}
