using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulosComerciales.Data;
using ModulosComerciales.Models;

namespace ModulosComerciales.Controllers
{
    public class ModulosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModulosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consultar todos los módulos
        public async Task<IActionResult> Index()
        {
            var modulos = await _context.Modulos.ToListAsync();
            return View(modulos);
        }

        // Mostrar formulario de registro
        public IActionResult Crear()
        {
            return View();
        }

        // Registrar un módulo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Modulo modulo)
        {
            if (!ModelState.IsValid)
            {
                return View(modulo);
            }

            _context.Modulos.Add(modulo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Mostrar formulario de edición
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modulo = await _context.Modulos.FindAsync(id);

            if (modulo == null)
            {
                return NotFound();
            }

            return View(modulo);
        }

        // Guardar cambios de un módulo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Modulo modulo)
        {
            if (id != modulo.ModuloId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(modulo);
            }

            _context.Update(modulo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Eliminar un módulo
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modulo = await _context.Modulos
                .FirstOrDefaultAsync(m => m.ModuloId == id);

            if (modulo == null)
            {
                return NotFound();
            }

            return View(modulo);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var modulo = await _context.Modulos.FindAsync(id);

            if (modulo != null)
            {
                bool tieneSolicitudes = await _context.SolicitudesAutorizacion
                    .AnyAsync(s => s.ModuloId == id);

                if (tieneSolicitudes)
                {
                    TempData["Error"] =
                        "No se puede eliminar el módulo porque tiene solicitudes asociadas.";

                    return RedirectToAction(nameof(Index));
                }

                _context.Modulos.Remove(modulo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
