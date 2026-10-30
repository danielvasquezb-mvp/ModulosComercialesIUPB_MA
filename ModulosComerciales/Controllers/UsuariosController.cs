using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulosComerciales.Data;
using ModulosComerciales.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ModulosComerciales.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // Listar usuarios
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        // Mostrar formulario
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // Registrar estudiante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            string nombre,
            string apellido,
            string correo,
            string password)
        {
            nombre = nombre?.Trim() ?? "";
            apellido = apellido?.Trim() ?? "";
            correo = correo?.Trim() ?? "";
            password ??= "";

            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError("nombre",
                    "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(apellido))
                ModelState.AddModelError("apellido",
                    "El apellido es obligatorio.");

            if (string.IsNullOrWhiteSpace(correo) ||
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(correo))
            {
                ModelState.AddModelError("correo",
                    "Ingresa un correo electrónico válido.");
            }

            if (password.Length < 8)
                ModelState.AddModelError("password",
                    "La contraseña debe tener al menos 8 caracteres.");

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == correo);

            if (correoExiste)
                ModelState.AddModelError("correo",
                    "Este correo electrónico ya está registrado.");

            if (!ModelState.IsValid)
                return View();

            var usuario = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                Rol = "Estudiante",
                Activo = true
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(
                usuario, password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Estudiante registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Mostrar formulario de inicio de sesión
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Validar credenciales e iniciar sesión
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string correo,
            string password,
            string? returnUrl = null)
        {
            correo = correo?.Trim() ?? "";
            password ??= "";

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo == correo && u.Activo);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "",
                    "Correo o contraseña incorrectos.");

                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Correo o contraseña incorrectos.");

                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.UsuarioId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre + " " + usuario.Apellido),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol)
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Cerrar sesión
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        // Página para usuarios sin permisos
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}