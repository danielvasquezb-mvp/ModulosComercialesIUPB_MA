using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulosComerciales.Data;
using ModulosComerciales.Models;

namespace ModulosComerciales.Controllers
{
    [Authorize]
    public class MarcasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarcasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consultar marcas: el estudiante solo ve la suya.
        public async Task<IActionResult> Index()
        {
            var marcas = await _context.Marcas
                .Include(m => m.Usuario)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return View(marcas);
        }

        // Mostrar formulario de registro.
        public async Task<IActionResult> Crear()
        {
            bool esAdministrador = User.IsInRole("Administrador");

            // Un estudiante solo puede tener una marca.
            if (!esAdministrador)
            {
                int usuarioId = ObtenerUsuarioId();

                bool yaTieneMarca = await _context.Marcas
                    .AnyAsync(m => m.UsuarioId == usuarioId);

                if (yaTieneMarca)
                {
                    TempData["Error"] =
                        "Ya tienes una marca registrada.";

                    return RedirectToAction(nameof(Index));
                }
            }

            await CargarUsuariosSiEsAdministrador();

            ViewBag.EsAdministrador = esAdministrador;

            return View();
        }

        // Registrar una marca.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Marca marca)
        {
            bool esAdministrador = User.IsInRole("Administrador");

            int usuarioId;

            if (esAdministrador)
            {
                // El administrador selecciona al estudiante.
                usuarioId = marca.UsuarioId;
            }
            else
            {
                // El estudiante siempre queda asociado a su propia cuenta.
                usuarioId = ObtenerUsuarioId();
                marca.UsuarioId = usuarioId;
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.UsuarioId == usuarioId &&
                    u.Rol == "Estudiante" &&
                    u.Activo);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "UsuarioId",
                    "El estudiante no existe o está inactivo.");
            }

            bool yaTieneMarca = await _context.Marcas
                .AnyAsync(m => m.UsuarioId == usuarioId);

            if (yaTieneMarca)
            {
                ModelState.AddModelError(
                    "UsuarioId",
                    "Este estudiante ya tiene una marca registrada.");
            }

            if (!ModelState.IsValid)
            {
                await CargarUsuariosSiEsAdministrador();
                ViewBag.EsAdministrador = esAdministrador;

                return View(marca);
            }

            marca.Usuario = usuario!;

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Mostrar formulario de edición.
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.MarcaId == id);

            if (marca == null)
            {
                return NotFound();
            }

            // El estudiante no puede editar marcas ajenas.
            if (!User.IsInRole("Administrador") &&
                marca.UsuarioId != ObtenerUsuarioId())
            {
                return Forbid();
            }

            await CargarUsuariosSiEsAdministrador();
            ViewBag.EsAdministrador = User.IsInRole("Administrador");

            return View(marca);
        }

        // Guardar cambios de una marca.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Marca marca)
        {
            if (id != marca.MarcaId)
            {
                return BadRequest();
            }

            var marcaExistente = await _context.Marcas
                .FirstOrDefaultAsync(m => m.MarcaId == id);

            if (marcaExistente == null)
            {
                return NotFound();
            }

            bool esAdministrador = User.IsInRole("Administrador");

            // Impedir que un estudiante modifique marcas ajenas.
            if (!esAdministrador &&
                marcaExistente.UsuarioId != ObtenerUsuarioId())
            {
                return Forbid();
            }

            // Solo el administrador puede cambiar el propietario.
            int usuarioId = esAdministrador
                ? marca.UsuarioId
                : marcaExistente.UsuarioId;

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.UsuarioId == usuarioId &&
                    u.Rol == "Estudiante" &&
                    u.Activo);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "UsuarioId",
                    "El estudiante no existe o está inactivo.");
            }

            bool marcaDuplicada = await _context.Marcas
                .AnyAsync(m =>
                    m.UsuarioId == usuarioId &&
                    m.MarcaId != id);

            if (marcaDuplicada)
            {
                ModelState.AddModelError(
                    "UsuarioId",
                    "Este estudiante ya tiene otra marca.");
            }

            if (!ModelState.IsValid)
            {
                await CargarUsuariosSiEsAdministrador();
                ViewBag.EsAdministrador = esAdministrador;

                return View(marca);
            }

            // Actualizar solo los campos permitidos.
            marcaExistente.Nombre = marca.Nombre;
            marcaExistente.Descripcion = marca.Descripcion;

            if (esAdministrador)
            {
                marcaExistente.UsuarioId = usuarioId;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Obtener el ID del usuario autenticado.
        private int ObtenerUsuarioId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // Cargar estudiantes solo para el administrador.
        private async Task CargarUsuariosSiEsAdministrador()
        {
            if (User.IsInRole("Administrador"))
            {
                ViewBag.Usuarios = await _context.Usuarios
                    .Where(u => u.Rol == "Estudiante" && u.Activo)
                    .ToListAsync();
            }
        }
    }
}
