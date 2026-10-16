using Microsoft.EntityFrameworkCore;
using ModulosComerciales.Models;

namespace ModulosComerciales.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Producto> Productos { get; set; }

        public DbSet<SolicitudAutorizacion>
            SolicitudesAutorizacion
        { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Un correo no puede repetirse.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // Cada estudiante puede registrar una marca.
            modelBuilder.Entity<Marca>()
                .HasIndex(m => m.UsuarioId)
                .IsUnique();

            // Relación Usuario - Marca.
            modelBuilder.Entity<Marca>()
                .HasOne(m => m.Usuario)
                .WithOne(u => u.Marca)
                .HasForeignKey<Marca>(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Marca - Producto.
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Precisión de los precios.
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            // Relación Marca - Solicitud.
            modelBuilder.Entity<SolicitudAutorizacion>()
                .HasOne(s => s.Marca)
                .WithMany(m => m.Solicitudes)
                .HasForeignKey(s => s.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Módulo - Solicitud.
            modelBuilder.Entity<SolicitudAutorizacion>()
                .HasOne(s => s.Modulo)
                .WithMany(m => m.Solicitudes)
                .HasForeignKey(s => s.ModuloId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
