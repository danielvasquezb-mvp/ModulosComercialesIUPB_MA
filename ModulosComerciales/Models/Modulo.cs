namespace ModulosComerciales.Models
{
    public class Modulo
    {
        public int ModuloId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Ubicacion { get; set; } = string.Empty;

        public string Estado { get; set; } = "Cerrado";

        public ICollection<SolicitudAutorizacion> Solicitudes
        { get; set; } = new List<SolicitudAutorizacion>();
    }
}
