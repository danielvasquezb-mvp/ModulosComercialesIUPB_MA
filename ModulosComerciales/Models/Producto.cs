namespace ModulosComerciales.Models
{
    public class Producto
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int MarcaId { get; set; }

        public Marca Marca { get; set; } = null!;
    }
}
