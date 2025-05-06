using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using OvertimeControlCodeFirst.Models;
using OvertimeControlCodeFirst.Data;


namespace OvertimeControlCodeFirst.Controllers
{
    public class AccountController : Controller
    {
        private readonly OvertimeDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(OvertimeDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserName == model.UserName);
                    if (usuario != null && usuario.Password == model.Password)
                    {
                        var auditoriaLogin = new LoginAudit
                        {
                            UserId = usuario.UserId,
                            User = usuario,
                            LoginDate = DateTime.Now,
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                        };
                        _context.LoginAudits.Add(auditoriaLogin);
                        await _context.SaveChangesAsync();

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, usuario.UserName),
                            new Claim("FirstName", usuario.FirstName),
                            new Claim("LastName", usuario.LastName),
                            new Claim("UserId", usuario.UserId.ToString()),
                            new Claim("Role", usuario.Role.Name),
                            new Claim("AreaId", usuario.AreaId.ToString() ?? string.Empty),
                            new Claim("SecretariatId", usuario.SecretariatId.ToString() ?? string.Empty)
                        };
                        var identity = new ClaimsIdentity(claims, "Cookies");
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync("Cookies", principal);

                        return RedirectToAction("Index", "Dashboard");
                    }
                    else ModelState.AddModelError("", "Nombre de usuario o contraseña incorrectos.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al intentar iniciar sesión. Por favor, inténtelo de nuevo más tarde.");
                    _logger.LogError(ex, "Error al intentar iniciar sesión.");
                }
            }
            return View(model);
        }

        public IActionResult Create()
        {
            var areaIdClaim = User.FindFirst("AreaId");

            if (areaIdClaim == null)
            {
                return Unauthorized();
            }

            var areaId = int.Parse(areaIdClaim.Value);
            var employees = _context.Employees
                .Where(e => e.AreaId == areaId)
                .ToList();

            ViewData["Empleados"] = employees;

            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}