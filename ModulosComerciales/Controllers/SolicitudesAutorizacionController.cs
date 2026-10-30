using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulosComerciales.Data;
using ModulosComerciales.Models;

namespace ModulosComerciales.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SolicitudesAutorizacionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesAutorizacionController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Consultar todas las solicitudes
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesAutorizacion
                .Include(s => s.Marca)
                .Include(s => s.Modulo)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        // Mostrar formulario para solicitar un módulo
        public async Task<IActionResult> Crear()
        {
            ViewBag.Marcas = await _context.Marcas.ToListAsync();

            ViewBag.Modulos = await _context.Modulos
                .Where(m => m.Estado == "Abierto")
                .ToListAsync();

            return View();
        }

        // Registrar una solicitud
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int marcaId, int moduloId)
        {
            var marca = await _context.Marcas.FindAsync(marcaId);
            var modulo = await _context.Modulos.FindAsync(moduloId);

            if (marca == null || modulo == null)
            {
                return NotFound();
            }

            if (modulo.Estado != "Abierto")
            {
                ModelState.AddModelError("",
                    "El módulo no está abierto para solicitudes.");
            }

            bool solicitudExistente =
                await _context.SolicitudesAutorizacion.AnyAsync(s =>
                    s.MarcaId == marcaId &&
                    s.ModuloId == moduloId &&
                    (s.Estado == "Pendiente" ||
                     s.Estado == "Aprobada"));

            if (solicitudExistente)
            {
                ModelState.AddModelError("",
                    "La marca ya tiene una solicitud pendiente " +
                    "o una autorización activa para este módulo.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Marcas = await _context.Marcas.ToListAsync();

                ViewBag.Modulos = await _context.Modulos
                    .Where(m => m.Estado == "Abierto")
                    .ToListAsync();

                return View();
            }

            var solicitud = new SolicitudAutorizacion
            {
                MarcaId = marcaId,
                ModuloId = moduloId,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            _context.SolicitudesAutorizacion.Add(solicitud);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Aprobar una solicitud
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var solicitud = await _context.SolicitudesAutorizacion
                .FindAsync(id);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Pendiente")
            {
                return BadRequest(
                    "Solo se pueden aprobar solicitudes pendientes.");
            }

            solicitud.Estado = "Aprobada";
            solicitud.FechaRespuesta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Rechazar una solicitud
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(
            int id, string? observacion)
        {
            var solicitud = await _context.SolicitudesAutorizacion
                .FindAsync(id);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Pendiente")
            {
                return BadRequest(
                    "Solo se pueden rechazar solicitudes pendientes.");
            }

            solicitud.Estado = "Rechazada";
            solicitud.FechaRespuesta = DateTime.UtcNow;
            solicitud.ObservacionAdministrador = observacion;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Finalizar el uso autorizado de un módulo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            var solicitud = await _context.SolicitudesAutorizacion
                .FindAsync(id);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Aprobada")
            {
                return BadRequest(
                    "Solo se puede finalizar una autorización aprobada.");
            }

            solicitud.Estado = "Finalizada";
            solicitud.FechaFinalizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
