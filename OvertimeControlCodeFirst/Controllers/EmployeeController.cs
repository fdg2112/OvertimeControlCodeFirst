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
            else if (rolClaim?.Value == "Secretario Hacienda")
            {
                var employees = _context.Employees
                    .Include(e => e.Area)
                    .Include(e => e.Secretariat)
                    .Include(e => e.SalaryCategory)
                    .Where(e => e.SecretariatId == 3)
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

        public async Task<IActionResult> GetEmployee(int? areaId = null)
        {
            var role= User.FindFirst("Role")?.Value;
            var areaIdClaim = User.FindFirst("AreaId")?.Value;
            var secretariatIdClaim = User.FindFirst("SecretariatId")?.Value;

            int? areaIdUser = string.IsNullOrEmpty(areaIdClaim) ? null : int.Parse(areaIdClaim);
            int? secretariaIdUsuario = string.IsNullOrEmpty(secretariatIdClaim) ? null : int.Parse(secretariatIdClaim);

            var employeesQuery = _context.Employees.AsQueryable();

            if (role == "Jefe de Área" && areaIdUser.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.AreaId == areaIdUser.Value);
            }
            else if (role == "Secretario" && secretariaIdUsuario.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.Area.SecretariatId == secretariaIdUsuario.Value);
                if (areaId.HasValue)
                {
                    employeesQuery = employeesQuery.Where(e => e.AreaId == areaId.Value);
                }
            }
            else if (role == "Secretario Hacienda")
            {
                employeesQuery = employeesQuery.Where(e => e.SecretariatId == 3);
                if (areaId.HasValue)
                {
                    employeesQuery = employeesQuery.Where(e => e.AreaId == areaId.Value);
                }
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
            var secretariatIdClaim = User.FindFirst("SecretariatId");
            var areas = _context.Areas.ToList();
            var secretariats = _context.Secretariats.ToList();
            var categories = _context.SalaryCategories.ToList();

            int? areaId = areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value) ? int.Parse(areaIdClaim.Value) : (int?)null;
            int? secretariatId = secretariatIdClaim != null && !string.IsNullOrEmpty(secretariatIdClaim.Value) ? int.Parse(secretariatIdClaim.Value) : (int?)null;

            var employees = _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .Include(e => e.SalaryCategory)
                .Where(e => (areaId.HasValue && e.AreaId == areaId) || (secretariatId.HasValue && e.SecretariatId == secretariatId))
                .ToList();

            ViewData["Areas"] = areas;
            ViewData["Secretaries"] = secretariats;
            ViewData["Categories"] = categories;
            ViewData["Employees"] = employees;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (_context.Employees.Any(e => e.RecordNumber == employee.RecordNumber))
            {
                return Json(new
                {
                    success = false,
                    message = "El número de legajo ya está registrado."
                });
            }
            if (employee.RecordNumber < 0 && employee.RecordNumber > 999)
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
                _context.Employees.Add(employee);
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
                message = "Error al registrar el employee."
            });
        }

        [HttpGet]
        public IActionResult GetAreasAndSecretariats()
        {
            var areaIdClaim = User.FindFirst("AreaId");
            var secretariatIdClaim = User.FindFirst("SecretariatId");

            int? areaId = null;
            if (areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value))
            {
                areaId = int.Parse(areaIdClaim.Value);
            }

            if (secretariatIdClaim == null || string.IsNullOrEmpty(secretariatIdClaim.Value))
            {
                return Json(new { error = "No se encontró el claim de Secretaría." });
            }

            int secretariatId = int.Parse(secretariatIdClaim.Value);

            var areas = _context.Areas
                .Where(a => a.SecretariatId == secretariatId && (!areaId.HasValue || a.AreaId == areaId))
                .Select(a => new { id = a.AreaId, name = a.Name })
                .ToList();

            var secretariats = _context.Secretariats
                .Where(s => s.SecretariatId == secretariatId)
                .Select(s => new { id = s.SecretariatId, name = s.Name })
                .ToList();

            var categories = _context.SalaryCategories
                .Select(c => new { id = c.SalaryCategoryId, name = c.Number })
                .ToList();

            return Json(new
            {
                areas,
                secretariats,
                categories,
                defaultAreaId = areaId,
                defaultSecretariatId = secretariatId
            });
        }

        [HttpGet]
        public IActionResult GetAreasBySecretariat(int secretariatId)
        {
            var areas = _context.Areas
                .Where(a => a.SecretariatId == secretariatId)
                .Select(a => new { a.AreaId, a.Name })
                .ToList();

            return Json(areas);
        }



        [HttpGet]
        public IActionResult CheckRecordNumber(int recordNumber)
        {
            var existingEmployee = _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .Where(e => e.RecordNumber == recordNumber)
                .Select(e => new
                {
                    Name = e.Name,
                    LastName = e.LastName,
                    AreaName = e.Area != null ? e.Area.Name : "Sin Área",
                    SecretariatName = e.Secretariat != null ? e.Secretariat.Name : "Sin Secretaría"
                })
                .FirstOrDefault();

            if (existingEmployee != null)
            {
                return Json(new
                {
                    exists = true,
                    employee = existingEmployee
                });
            }

            return Json(new { exists = false });
        }

        [HttpGet]
        public IActionResult GetEmployeeById(int id)
        {
            var employee = _context.Employees
                .Include(e => e.SalaryCategory)
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }
            var categories = _context.SalaryCategories
                .Select(c => new { salaryCategoryId = c.SalaryCategoryId, number = c.Number })
                .ToList();

            var areas = _context.Areas
                .Select(a => new { areaId = a.AreaId, name = a.Name, secretariatId = a.SecretariatId })
                .ToList();

            var secretariats = _context.Secretariats
                .Select(s => new { secretariatId = s.SecretariatId, name = s.Name })
                .ToList();

            return Json(new
            {
                name = employee.Name,
                lastname = employee.LastName,
                categoryId = employee.SalaryCategoryId,
                areaId = employee.AreaId,
                secretariatId = employee.SecretariatId,
                categories,
                areas,
                secretariats
            });
        }

        [HttpPost]
        public IActionResult EditEmployee(int id, Employee model)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }
            employee.Name = model.Name;
            employee.LastName = model.LastName;
            employee.SalaryCategoryId = model.SalaryCategoryId;
            employee.AreaId = model.AreaId;
            employee.SecretariatId = model.SecretariatId;

            _context.Employees
            .Where(e => e.EmployeeId == id)
            .ExecuteUpdate(setters => setters
                .SetProperty(e => e.Name, model.Name)
                .SetProperty(e => e.LastName, model.LastName)
                .SetProperty(e => e.SalaryCategoryId, model.SalaryCategoryId)
                .SetProperty(e => e.AreaId, model.AreaId)
                .SetProperty(e => e.SecretariatId, model.SecretariatId)
            );

            return Json(new { success = true, message = "Empleado actualizado correctamente" });
        }

    }
}
