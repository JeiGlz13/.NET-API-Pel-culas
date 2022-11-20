using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class SalaDeCineCercanoFiltroDTO
    {
        [Range(-90, 90)]
        public double Latitud { get; set; }
        [Range(-180, 180)]
        public double Longitud { get; set; }
        private int distanciaEnKM = 10;
        private int distanciaMaximaKM = 50;

        public int DistanciaEnKM
        {
            get { return distanciaEnKM; }
            set { distanciaEnKM = (value > distanciaMaximaKM) ? distanciaMaximaKM : value; }
        }
    }
}
