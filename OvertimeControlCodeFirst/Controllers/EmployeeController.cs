using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OvertimeControlCodeFirst.Data;
using OvertimeControlCodeFirst.Models;

namespace OvertimeControlCodeFirst.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly OvertimeDbContext _context;

        public EmployeeController(OvertimeDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var areaIdClaim = User.FindFirst("AreaId");
            var secretariatIdClaim = User.FindFirst("SecretariatId");
            var rolClaim = User.FindFirst("Role");

            if (rolClaim?.Value == "Intendente")
            {
                var employees = _context.Employees
                    .Include(e => e.Area)
                    .Include(e => e.Secretariat)
                    .Include(e => e.SalaryCategory)
                    .ToList();

                // Paginación
                var totalEmployees = employees.Count();
                var paginatedEmployees = employees
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalEmployees / pageSize);

                ViewData["Employees"] = paginatedEmployees;
                ViewData["TotalPages"] = totalPages;
                ViewData["CurrentPage"] = page;
                ViewData["PageSize"] = pageSize;
            }
            else
            {
                int? areaId = null;
                if (areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value))
                {
                    areaId = int.Parse(areaIdClaim.Value);
                }

                if (secretariatIdClaim == null || string.IsNullOrEmpty(secretariatIdClaim.Value))
                {
                    return View("Error", new { message = "No se encontró el claim de Secretaría." });
                }
                int secretariatId = int.Parse(secretariatIdClaim.Value);

                var query = _context.Employees
                    .Include(e => e.Area)
                    .Include(e => e.Secretariat)
                    .Include(e => e.SalaryCategory)
                    .Where(e =>
                        (areaId.HasValue && e.AreaId == areaId) ||
                        (!areaId.HasValue && e.SecretariatId == secretariatId))
                    .AsQueryable();

                var totalEmployees = query.Count();
                var employees = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalEmployees / pageSize);

                ViewData["Employees"] = employees;
                ViewData["TotalPages"] = totalPages;
                ViewData["CurrentPage"] = page;
                ViewData["PageSize"] = pageSize;
            }

            return View();
        }

        public async Task<IActionResult> GetEmployy(int? areaId = null)
        {
            var role= User.FindFirst("Rol")?.Value;
            var areaIdClaim = User.FindFirst("AreaId")?.Value;
            var secretariaIdClaim = User.FindFirst("SecretariatId")?.Value;

            int? areaIdUsuario = string.IsNullOrEmpty(areaIdClaim) ? null : int.Parse(areaIdClaim);
            int? secretariaIdUsuario = string.IsNullOrEmpty(secretariaIdClaim) ? null : int.Parse(secretariaIdClaim);

            var employeesQuery = _context.Employees.AsQueryable();

            if (role== "Jefe de Área" && areaIdUsuario.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.AreaId == areaIdUsuario.Value);
            }
            else if (role== "Secretario" && secretariaIdUsuario.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.Area.SecretariatId == secretariaIdUsuario.Value);
                if (areaId.HasValue)
                {
                    employeesQuery = employeesQuery.Where(e => e.AreaId == areaId.Value);
                }
            }
            else if (role== "Intendente" || role== "Secretario Hacienda")
            {
                // Intendentes y Secretarios de Hacienda no tienen filtro inicial por ahora
            }
            else if (areaId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.AreaId == areaId.Value);
            }

            var employees = await employeesQuery
                .Select(e => new
                {
                    e.EmployeeId,
                    e.RecordNumber,
                    e.LastName,
                    e.Name,
                    categoryNumber = e.SalaryCategory.Number,
                    areaName = e.Area.Name,
                    secretariatName = e.Secretariat.Name,
                })
                .ToListAsync();

            return Json(employees);
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            var areaIdClaim = User.FindFirst("AreaId");
            var secretariaIdClaim = User.FindFirst("SecretariatId");
            var areas = _context.Areas.ToList();
            var secretarias = _context.Secretariats.ToList();
            var categorias = _context.SalaryCategories.ToList();

            int? areaId = areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value) ? int.Parse(areaIdClaim.Value) : (int?)null;
            int? secretariaId = secretariaIdClaim != null && !string.IsNullOrEmpty(secretariaIdClaim.Value) ? int.Parse(secretariaIdClaim.Value) : (int?)null;

            var employees = _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .Include(e => e.SalaryCategory)
                .Where(e => (areaId.HasValue && e.AreaId == areaId) || (secretariaId.HasValue && e.SecretariatId == secretariaId))
                .ToList();

            ViewData["Areas"] = areas;
            ViewData["Secretarias"] = secretarias;
            ViewData["Categorias"] = categorias;
            ViewData["Empleados"] = employees;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(Employee empleado)
        {
            if (_context.Employees.Any(e => e.RecordNumber == empleado.RecordNumber))
            {
                return Json(new
                {
                    success = false,
                    message = "El número de legajo ya está registrado."
                });
            }
            if (empleado.RecordNumber < 0 && empleado.RecordNumber > 999)
            {
                return Json(new
                {
                    success = false,
                    message = "El número de legajo debe ser un número de 3 dígitos."
                });
            }

            ModelState.Remove("Area");
            ModelState.Remove("Secretariat");
            ModelState.Remove("SalaryCategory");

            if (ModelState.IsValid)
            {
                _context.Employees.Add(empleado);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Empleado registrado exitosamente."
                });
            }

            return Json(new
            {
                success = false,
                message = "Error al registrar el empleado."
            });
        }

        [HttpGet]
        public IActionResult GetAreasAndSecretarias()
        {
            var areaIdClaim = User.FindFirst("AreaId");
            var secretariaIdClaim = User.FindFirst("SecretariatId");

            int? areaId = null;
            if (areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value))
            {
                areaId = int.Parse(areaIdClaim.Value);
            }

            if (secretariaIdClaim == null || string.IsNullOrEmpty(secretariaIdClaim.Value))
            {
                return Json(new { error = "No se encontró el claim de Secretaría." });
            }

            int secretariaId = int.Parse(secretariaIdClaim.Value);

            var areas = _context.Areas
                .Where(a => a.SecretariatId == secretariaId && (!areaId.HasValue || a.AreaId == areaId))
                .Select(a => new { id = a.AreaId, nombre = a.Name })
                .ToList();

            var secretarias = _context.Secretariats
                .Where(s => s.SecretariatId == secretariaId)
                .Select(s => new { id = s.SecretariatId, nombre = s.Name })
                .ToList();

            var categorias = _context.SalaryCategories
                .Select(c => new { id = c.SalaryCategoryId, nombre = c.Number })
                .ToList();

            return Json(new
            {
                areas,
                secretarias,
                categorias,
                defaultAreaId = areaId,
                defaultSecretariaId = secretariaId
            });
        }

        [HttpGet]
        public IActionResult CheckLegajo(int legajo)
        {
            var empleadoExistente = _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .Where(e => e.RecordNumber == legajo)
                .Select(e => new
                {
                    Name = e.Name,
                    LastName = e.LastName,
                    AreaNombre = e.Area != null ? e.Area.Name : "Sin Área",
                    SecretariaNombre = e.Secretariat != null ? e.Secretariat.Name : "Sin Secretaría"
                })
                .FirstOrDefault();

            if (empleadoExistente != null)
            {
                return Json(new
                {
                    exists = true,
                    empleado = empleadoExistente
                });
            }

            return Json(new { exists = false });
        }

        [HttpGet]
        public IActionResult GetEmpleadoById(int id)
        {
            var empleado = _context.Employees
                .Include(e => e.SalaryCategory)
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (empleado == null)
            {
                return NotFound();
            }
            var categorias = _context.SalaryCategories.Select(c => new { c.SalaryCategoryId, c.Number }).ToList();
            var areas = _context.Areas.Select(a => new { a.AreaId, a.Name }).ToList();
            var secretarias = _context.Secretariats.Select(s => new { s.SecretariatId, s.Name }).ToList();

            return Json(new
            {
                nombre = empleado.Name,
                apellido = empleado.LastName,
                categoriaId = empleado.SalaryCategoryId,
                areaId = empleado.AreaId,
                secretariaId = empleado.SecretariatId,
                categorias,
                areas,
                secretarias
            });
        }

        [HttpPost]
        public IActionResult EditEmpleado(int id, Employee model)
        {
            var empleado = _context.Employees.FirstOrDefault(e => e.EmployeeId == id);
            if (empleado == null)
            {
                return NotFound();
            }
            empleado.Name = model.Name;
            empleado.LastName = model.LastName;
            empleado.SalaryCategoryId = model.SalaryCategoryId;
            empleado.AreaId = model.AreaId;
            empleado.SecretariatId = model.SecretariatId;

            _context.SaveChanges();

            return Json(new { success = true, message = "Empleado actualizado correctamente" });
        }

    }
}
