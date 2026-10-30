using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ModulosComerciales.Models
{
    public class Marca
    {
        public int MarcaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        public ICollection<Producto> Productos
        { get; set; } = new List<Producto>();

        public ICollection<SolicitudAutorizacion> Solicitudes
        { get; set; } = new List<SolicitudAutorizacion>();
    }
}
