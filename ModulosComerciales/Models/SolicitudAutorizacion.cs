namespace ModulosComerciales.Models
{
    public class SolicitudAutorizacion
    {
        public int SolicitudAutorizacionId { get; set; }

        public int MarcaId { get; set; }

        public Marca Marca { get; set; } = null!;

        public int ModuloId { get; set; }

        public Modulo Modulo { get; set; } = null!;

        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public DateTime? FechaRespuesta { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        public string? ObservacionAdministrador { get; set; }
    }
}
