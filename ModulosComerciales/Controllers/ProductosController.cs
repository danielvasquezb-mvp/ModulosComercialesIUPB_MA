using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulosComerciales.Data;
using ModulosComerciales.Models;

namespace ModulosComerciales.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consultar productos: el estudiante solo ve los de su marca.
        public async Task<IActionResult> Index()
        {
            var consulta = _context.Productos
                .Include(p => p.Marca)
                .AsQueryable();

            if (!EsAdministrador())
            {
                int usuarioId = ObtenerUsuarioId();

                consulta = consulta.Where(
                    p => p.Marca.UsuarioId == usuarioId);
            }

            var productos = await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }

        // Mostrar formulario de registro.
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            if (!EsAdministrador())
            {
                var marca = await ObtenerMarcaDelEstudiante();

                if (marca == null)
                {
                    TempData["Error"] =
                        "Primero debes registrar tu marca.";

                    return RedirectToAction("Crear", "Marcas");
                }
            }

            await CargarMarcas();

            return View();
        }

        // Registrar producto.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto)
        {
            ModelState.Remove(nameof(Producto.Marca));

            if (!EsAdministrador())
            {
                // Ignorar cualquier MarcaId enviado por el formulario.
                var marcaPropia = await ObtenerMarcaDelEstudiante();

                if (marcaPropia == null)
                {
                    TempData["Error"] =
                        "Primero debes registrar tu marca.";

                    return RedirectToAction("Crear", "Marcas");
                }

                producto.MarcaId = marcaPropia.MarcaId;
            }

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.MarcaId == producto.MarcaId);

            if (marca == null ||
                (!EsAdministrador() &&
                 marca.UsuarioId != ObtenerUsuarioId()))
            {
                ModelState.AddModelError(
                    nameof(Producto.MarcaId),
                    "La marca seleccionada no es válida.");
            }

            if (producto.Precio <= 0)
            {
                ModelState.AddModelError(
                    nameof(Producto.Precio),
                    "El precio debe ser mayor que cero.");
            }

            if (!ModelState.IsValid)
            {
                await CargarMarcas();
                return View(producto);
            }

            producto.Marca = marca!;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Producto registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Mostrar formulario de edición.
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
                return NotFound();

            if (!EsAdministrador() &&
                producto.Marca.UsuarioId != ObtenerUsuarioId())
            {
                return Forbid();
            }

            await CargarMarcas();

            return View(producto);
        }

        // Guardar cambios del producto.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id, Producto producto)
        {
            if (id != producto.ProductoId)
                return BadRequest();

            var productoExistente = await _context.Productos
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (productoExistente == null)
                return NotFound();

            if (!EsAdministrador() &&
                productoExistente.Marca.UsuarioId != ObtenerUsuarioId())
            {
                return Forbid();
            }

            ModelState.Remove(nameof(Producto.Marca));

            // El estudiante conserva la marca original.
            if (!EsAdministrador())
            {
                producto.MarcaId = productoExistente.MarcaId;
            }

            var marca = await _context.Marcas
                .FirstOrDefaultAsync(m => m.MarcaId == producto.MarcaId);

            if (marca == null ||
                (!EsAdministrador() &&
                 marca.UsuarioId != ObtenerUsuarioId()))
            {
                ModelState.AddModelError(
                    nameof(Producto.MarcaId),
                    "La marca seleccionada no es válida.");
            }

            if (producto.Precio <= 0)
            {
                ModelState.AddModelError(
                    nameof(Producto.Precio),
                    "El precio debe ser mayor que cero.");
            }

            if (!ModelState.IsValid)
            {
                await CargarMarcas();
                return View(producto);
            }

            productoExistente.Nombre = producto.Nombre;
            productoExistente.Descripcion = producto.Descripcion;
            productoExistente.Precio = producto.Precio;

            if (EsAdministrador())
            {
                productoExistente.MarcaId = producto.MarcaId;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Producto actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Mostrar confirmación de eliminación.
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
                return NotFound();

            if (!EsAdministrador() &&
                producto.Marca.UsuarioId != ObtenerUsuarioId())
            {
                return Forbid();
            }

            return View(producto);
        }

        // Confirmar eliminación.
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
                return NotFound();

            if (!EsAdministrador() &&
                producto.Marca.UsuarioId != ObtenerUsuarioId())
            {
                return Forbid();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Producto eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Cargar todas las marcas para el administrador
        // o solo la marca propia para el estudiante.
        private async Task CargarMarcas()
        {
            var consulta = _context.Marcas.AsQueryable();

            if (!EsAdministrador())
            {
                int usuarioId = ObtenerUsuarioId();

                consulta = consulta.Where(m => m.UsuarioId == usuarioId);
            }

            ViewBag.Marcas = await consulta
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        private async Task<Marca?> ObtenerMarcaDelEstudiante()
        {
            int usuarioId = ObtenerUsuarioId();

            return await _context.Marcas
                .FirstOrDefaultAsync(m => m.UsuarioId == usuarioId);
        }

        private int ObtenerUsuarioId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private bool EsAdministrador()
        {
            return User.IsInRole("Administrador");
        }
    }
}
