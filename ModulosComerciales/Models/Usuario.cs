namespace ModulosComerciales.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Rol { get; set; } = "Estudiante";

        public bool Activo { get; set; } = true;

        public Marca? Marca { get; set; }
    }
}

